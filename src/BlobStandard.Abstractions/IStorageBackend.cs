// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using BlobStandard.Models;

namespace BlobStandard;

public interface IStorageBackend
{
    IAsyncEnumerable<BucketDetails> ListBucketsAsync(CancellationToken cancellationToken = default);

    Task<bool> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default);

    DownloadBlobOptions CreateDownloadBlobOptions() => new();
    Task<BlobDownloadResponse> DownloadBlobAsync(string bucketName, string blobName, DownloadBlobOptions? options = null, CancellationToken cancellationToken = default);

    GetBlobDetailsOptions CreateGetBlobDetailsOptions() => new();
    Task<BlobDetails?> GetBlobDetailsAsync(string bucketName, string blobName, GetBlobDetailsOptions? options = null, CancellationToken cancellationToken = default);

    DeleteBlobOptions CreateDeleteBlobOptions() => new();
    Task<bool> DeleteBlobAsync(string bucketName, string blobName, DeleteBlobOptions? options = null, CancellationToken cancellationToken = default);

    ListBlobsOptions CreateListBlobsOptions() => new();
    IAsyncEnumerable<ListItemBlob> ListBlobsAsync(string bucketName, string prefix, ListBlobsOptions? options = null, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ListItemBase> ListBlobsByHierarchyAsync(string bucketName, string prefix, ListBlobsOptions? options = null, CancellationToken cancellationToken = default);

    UploadBlobOptions CreateUploadBlobOptions() => new();
    Task<BlobUploader> StartUploadBlobAsync(string bucketName, string blobName, UploadBlobOptions? options = null, CancellationToken cancellationToken = default);
}
