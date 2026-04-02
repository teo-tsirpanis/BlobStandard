// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using BlobStandard.Models;

namespace BlobStandard;

/// <summary>
/// Defines the contract for a storage backend that provides blob storage operations.
/// Implementations abstract over different cloud or local storage providers.
/// </summary>
public interface IStorageBackend
{
    /// <summary>
    /// Creates a <see cref="DownloadBlobOptions"/> instance for use with <see cref="DownloadBlobAsync"/>.
    /// </summary>
    /// <remarks>
    /// A storage backend may return a derived type, allowing you to set backend-specific configuration.
    /// You must use instances returned by this method only with the same storage backend.
    /// </remarks>
    DownloadBlobOptions CreateDownloadBlobOptions() => new();

    /// <summary>
    /// Downloads a blob from the storage backend.
    /// </summary>
    /// <param name="bucketName">The name of the bucket containing the blob.</param>
    /// <param name="blobName">The name of the blob to download.</param>
    /// <param name="options">Optional download options, such as a byte range. If <see langword="null"/>, defaults are used.</param>
    /// <param name="cancellationToken">Used to cancel the operation.</param>
    /// <returns>A <see cref="BlobDownloadResponse"/> containing the blob's content and metadata.</returns>
    Task<BlobDownloadResponse> DownloadBlobAsync(string bucketName, string blobName, DownloadBlobOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a <see cref="GetBlobDetailsOptions"/> instance for use with <see cref="GetBlobDetailsAsync"/>.
    /// </summary>
    /// <remarks>
    /// A storage backend may return a derived type, allowing you to set backend-specific configuration.
    /// You must use instances returned by this method only with the same storage backend.
    /// </remarks>
    GetBlobDetailsOptions CreateGetBlobDetailsOptions() => new();

    /// <summary>
    /// Retrieves metadata for a blob without downloading its content.
    /// </summary>
    /// <param name="bucketName">The name of the bucket containing the blob.</param>
    /// <param name="blobName">The name of the blob.</param>
    /// <param name="options">Optional options. If <see langword="null"/>, defaults are used.</param>
    /// <param name="cancellationToken">Used to cancel the operation.</param>
    /// <returns>A <see cref="BlobDetails"/> object, or <see langword="null"/> if the blob does not exist.</returns>
    Task<BlobDetails?> GetBlobDetailsAsync(string bucketName, string blobName, GetBlobDetailsOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a <see cref="DeleteBlobOptions"/> instance for use with <see cref="DeleteBlobAsync"/>.
    /// </summary>
    /// <remarks>
    /// A storage backend may return a derived type, allowing you to set backend-specific configuration.
    /// You must use instances returned by this method only with the same storage backend.
    /// </remarks>
    DeleteBlobOptions CreateDeleteBlobOptions() => new();

    /// <summary>
    /// Deletes a blob from a bucket.
    /// </summary>
    /// <param name="bucketName">The name of the bucket containing the blob.</param>
    /// <param name="blobName">The name of the blob to delete.</param>
    /// <param name="options">Optional options. If <see langword="null"/>, defaults are used.</param>
    /// <param name="cancellationToken">Used to cancel the operation.</param>
    /// <returns><see langword="true"/> if the blob was deleted; <see langword="false"/> if it did not exist.</returns>
    Task<bool> DeleteBlobAsync(string bucketName, string blobName, DeleteBlobOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a <see cref="ListBlobsOptions"/> instance for use with <see cref="ListBlobsAsync"/> or <see cref="ListBlobsByHierarchyAsync"/>.
    /// </summary>
    /// <remarks>
    /// A storage backend may return a derived type, allowing you to set backend-specific configuration.
    /// You must use instances returned by this method only with the same storage backend.
    /// </remarks>
    ListBlobsOptions CreateListBlobsOptions() => new();

    /// <summary>
    /// Asynchronously enumerates blobs in a bucket whose names start with the given prefix, returning a flat list.
    /// </summary>
    /// <param name="bucketName">The name of the bucket to list.</param>
    /// <param name="prefix">The prefix that blob names must start with.</param>
    /// <param name="options">Optional listing options. If <see langword="null"/>, defaults are used.</param>
    /// <param name="cancellationToken">Used to cancel the operation.</param>
    /// <returns>An async sequence of <see cref="ListItemBlob"/> objects.</returns>
    IAsyncEnumerable<ListItemBlob> ListBlobsAsync(string bucketName, string prefix, ListBlobsOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously enumerates blobs and common prefixes (virtual directories) in a bucket,
    /// returning results in a hierarchical manner delimited by a prefix.
    /// </summary>
    /// <param name="bucketName">The name of the bucket to list.</param>
    /// <param name="prefix">The prefix used to scope and group results.</param>
    /// <param name="options">Optional listing options. If <see langword="null"/>, defaults are used.</param>
    /// <param name="cancellationToken">Used to cancel the operation.</param>
    /// <returns>An async sequence of <see cref="ListItemBase"/> objects, each being either a <see cref="ListItemBlob"/> or a <see cref="ListItemPrefix"/>.</returns>
    /// <remarks>
    /// For file system backends, this method performs a directory listing, returning files as blobs and subdirectories as prefixes.
    /// For object storage backends, this method performs a hierarchical listing, with the delimiter being set to <c>/</c>. Backends
    /// may support setting custom delimiters via <paramref name="options"/>.
    /// </remarks>
    IAsyncEnumerable<ListItemBase> ListBlobsByHierarchyAsync(string bucketName, string prefix, ListBlobsOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a <see cref="UploadBlobOptions"/> instance for use with <see cref="StartUploadBlobAsync"/>.
    /// </summary>
    /// <remarks>
    /// A storage backend may return a derived type, allowing you to set backend-specific configuration.
    /// You must use instances returned by this method only with the same storage backend.
    /// </remarks>
    UploadBlobOptions CreateUploadBlobOptions() => new();

    /// <summary>
    /// Begins an upload of a new blob to a bucket.
    /// </summary>
    /// <param name="bucketName">The name of the destination bucket.</param>
    /// <param name="blobName">The name to assign to the uploaded blob.</param>
    /// <param name="options">Optional upload options. If <see langword="null"/>, defaults are used.</param>
    /// <param name="cancellationToken">Used to cancel the operation.</param>
    /// <returns>A <see cref="BlobUploader"/> whose <see cref="BlobUploader.Writer"/> can be used to stream the blob's content.</returns>
    Task<BlobUploader> StartUploadBlobAsync(string bucketName, string blobName, UploadBlobOptions? options = null, CancellationToken cancellationToken = default);
}
