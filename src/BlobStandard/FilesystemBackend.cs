// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using System.IO.Pipelines;
using BlobStandard.Models;
using BlobStandard.Utilities;
using Microsoft.Win32.SafeHandles;

namespace BlobStandard;

/// <summary>
/// Provides an <see cref="IStorageBackend"/> implementation backed by the local filesystem.
/// </summary>
/// <remarks>
/// The <c>bucketName</c> parameter of all methods must be an empty string.
/// Blob names are full filesystem paths.
/// </remarks>
public partial class FilesystemBackend : IStorageBackend
{
    private static void ValidateBucket(string bucketName)
    {
        ArgumentNullException.ThrowIfNull(bucketName);
        if (bucketName.Length != 0)
            throw new ArgumentException("The filesystem backend does not support buckets. The bucket name must be an empty string.", nameof(bucketName));
    }

    private static void ValidateBlobName(string blobName)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobName);
        // TODO: Prohibit names that end with the reserved temporary blob suffix.
        // We might need to put this check in a more provider-agnostic place.
    }

    private static void ValidatePrefix(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        if (prefix.Length != 0 && !Path.EndsInDirectorySeparator(prefix))
            throw new ArgumentException($"The filesystem backend requires the prefix to end with '{Path.DirectorySeparatorChar}' or be empty.", nameof(prefix));
    }

    private static BlobDetails GetBlobDetailsInternal(SafeFileHandle fileHandle)
    {
        return new BlobDetails
        {
            Size = RandomAccess.GetLength(fileHandle),
            LastModified = File.GetLastWriteTimeUtc(fileHandle),
        };
    }

    /// <inheritdoc/>
    public Task<BlobDownloadResponse> DownloadBlobAsync(string bucketName, string blobName, DownloadBlobOptions? options = null, CancellationToken cancellationToken = default)
    {
        ValidateBucket(bucketName);
        ValidateBlobName(blobName);
        long rangeStart = options?.RangeStart ?? 0;
        ArgumentOutOfRangeException.ThrowIfNegative(rangeStart, nameof(options.RangeStart));
        if (options?.RangeCount is { } rc)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(rc, nameof(options.RangeCount));
        }
        cancellationToken.ThrowIfCancellationRequested();

        var fs = new FileStream(blobName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1, FileOptions.Asynchronous);
        try
        {
            var details = GetBlobDetailsInternal(fs.SafeFileHandle);

            if (rangeStart is not 0)
            {
                fs.Seek(rangeStart, SeekOrigin.Begin);
            }

            Stream contentStream = options?.RangeCount is { } rangeCount
                ? new LimitedReadStream(fs, rangeCount)
                : fs;

            var result = Task.FromResult(new BlobDownloadResponse
            {
                Content = PipeReader.Create(contentStream),
                Details = details,
            });
            fs = null; // Release ownership of the FileStream. It will be disposed when the content pipe reader is completed.
            return result;
        }
        finally
        {
            fs?.Dispose();
        }
    }

    /// <inheritdoc/>
    public async Task<BlobDetails?> GetBlobDetailsAsync(string bucketName, string blobName, GetBlobDetailsOptions? options = null, CancellationToken cancellationToken = default)
    {
        ValidateBucket(bucketName);
        ValidateBlobName(blobName);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var fileHandle = File.OpenHandle(blobName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return GetBlobDetailsInternal(fileHandle);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteBlobAsync(string bucketName, string blobName, DeleteBlobOptions? options = null, CancellationToken cancellationToken = default)
    {
        ValidateBucket(bucketName);
        ValidateBlobName(blobName);
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            using var fileHandle = File.OpenHandle(blobName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, FileOptions.DeleteOnClose);
            return true;
        }
        catch (FileNotFoundException)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ListItemBlob> ListBlobsAsync(string bucketName, string prefix, ListBlobsOptions? options = null, CancellationToken cancellationToken = default)
    {
        ValidateBucket(bucketName);
        ValidatePrefix(prefix);
        cancellationToken.ThrowIfCancellationRequested();

        return ListBlobsInternal(prefix, recurse: true).Cast<ListItemBlob>();
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<ListItemBase> ListBlobsByHierarchyAsync(string bucketName, string prefix, ListBlobsOptions? options = null, CancellationToken cancellationToken = default)
    {
        ValidateBucket(bucketName);
        ValidatePrefix(prefix);
        cancellationToken.ThrowIfCancellationRequested();

        return ListBlobsInternal(prefix, recurse: false);
    }

    /// <inheritdoc/>
    public async Task<BlobUploader> StartUploadBlobAsync(string bucketName, string blobName, UploadBlobOptions? options = null, CancellationToken cancellationToken = default)
    {
        ValidateBucket(bucketName);
        ValidateBlobName(blobName);
        cancellationToken.ThrowIfCancellationRequested();

        string? directory = Path.GetDirectoryName(blobName);
        if (directory is { Length: > 0 })
            Directory.CreateDirectory(directory);

        return FilesystemBlobUploader.Create(blobName, options);
    }

    private sealed class FilesystemBlobUploader : BlobUploader
    {
        private FilesystemBlobUploader(PipeWriter writer) : base(writer) { }

        public static FilesystemBlobUploader Create(string blobName, UploadBlobOptions? options)
        {
            var pipe = new Pipe();
            _ = options?.AllowPartialReads ?? false
                ? WriteDirectAsync(pipe.Reader, blobName, options)
                : WriteAtomicAsync(pipe.Reader, blobName, options);
            return new FilesystemBlobUploader(pipe.Writer);
        }

        private static void DeleteTemporaryFile(string tempPath)
        {
            try
            {
                File.Delete(tempPath);
            }
            catch
            {
                // Best effort cleanup. If this fails, the temp file will be left on disk, but we don't want to throw an exception that would crash the process.
            }
        }

        // TODO: Directly write to SafeFileHandle.

        private static async Task WriteDirectAsync(PipeReader reader, string blobName, UploadBlobOptions options)
        {
            var fileMode = options.FailIfExists ? FileMode.CreateNew : FileMode.Create;
            bool committed = false;
            try
            {
                await using var fs = new FileStream(blobName, fileMode, FileAccess.Write, FileShare.Read, 1, FileOptions.Asynchronous);
                await reader.CopyToAsync(fs);
                await fs.FlushAsync();
                committed = true;
            }
            catch (Exception e)
            {
                DeleteTemporaryFile(blobName);
                await reader.CompleteAsync(e);
            }
            if (committed)
            {
                await reader.CompleteAsync();
            }
        }

        private static async Task WriteAtomicAsync(PipeReader reader, string blobName, UploadBlobOptions? options)
        {
            string tempPath = blobName + "." + Guid.NewGuid().ToString("N") + ".tmp";
            bool committed = false;
            try
            {
                await using (var fs = new FileStream(tempPath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.Asynchronous))
                {
                    await reader.CopyToAsync(fs);
                    await fs.FlushAsync();
                }
                File.Move(tempPath, blobName, overwrite: !(options?.FailIfExists ?? false));
                committed = true;
            }
            catch (Exception e)
            {
                DeleteTemporaryFile(tempPath);
                await reader.CompleteAsync(e);
            }
            if (committed)
            {
                await reader.CompleteAsync();
            }
        }
    }
}
