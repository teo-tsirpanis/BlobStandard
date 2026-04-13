// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using System.IO.Pipelines;
using BlobStandard.Models;
using BlobStandard.Utilities;
using Microsoft.Win32.SafeHandles;

namespace BlobStandard;

/// <summary>
/// Provides an <see cref="IStorageBackend"/> implementation backed by the local file system.
/// </summary>
/// <remarks>
/// The <c>bucketName</c> parameter of all methods must be an empty string.
/// Blob names are full file system paths.
/// </remarks>
public partial class FileSystemBackend : IStorageBackend
{
    private static void ValidateBucket(string bucketName)
    {
        ArgumentNullException.ThrowIfNull(bucketName);
        if (bucketName.Length != 0)
            throw new ArgumentException("The file system backend does not support buckets. The bucket name must be an empty string.", nameof(bucketName));
    }

    private static void ValidateBlobName(string blobName)
    {
        BackendUtilities.ValidateBlobName(blobName);
    }

    private static void ValidatePrefix(string prefix)
    {
        ArgumentNullException.ThrowIfNull(prefix);
        if (prefix.Length != 0 && !Path.EndsInDirectorySeparator(prefix))
            throw new ArgumentException($"The file system backend requires the prefix to end with '{Path.DirectorySeparatorChar}' or be empty.", nameof(prefix));
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

        return Uploader.Create(blobName, options);
    }
}
