// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Models;

/// <summary>
/// Contains options for <see cref="IStorageBackend.DeleteBlobAsync"/>.
/// </summary>
/// <seealso cref="IStorageBackend.CreateDeleteBlobOptions"/>
public class DeleteBlobOptions;

/// <summary>
/// Contains options for <see cref="IStorageBackend.DownloadBlobAsync"/>.
/// </summary>
/// <seealso cref="IStorageBackend.CreateDownloadBlobOptions"/>
public class DownloadBlobOptions
{
    /// <summary>
    /// The zero-based byte offset at which to begin the download.
    /// Defaults to <c>0</c> (the beginning of the blob).
    /// </summary>
    public long RangeStart { get; set; }

    /// <summary>
    /// The number of bytes to download, or <see langword="null"/> to download to the end of the blob.
    /// Defaults to <see langword="null"/>.
    /// </summary>
    public long? RangeCount { get; set; }
}

/// <summary>
/// Contains options for <see cref="IStorageBackend.GetBlobDetailsAsync"/>.
/// </summary>
/// <seealso cref="IStorageBackend.CreateGetBlobDetailsOptions"/>
public class GetBlobDetailsOptions;

/// <summary>
/// Contains options for <see cref="IStorageBackend.ListBlobsAsync"/> and <see cref="IStorageBackend.ListBlobsByHierarchyAsync"/>.
/// </summary>
/// <seealso cref="IStorageBackend.CreateListBlobsOptions"/>
public class ListBlobsOptions;

/// <summary>
/// Contains options for <see cref="IStorageBackend.StartUploadBlobAsync"/>.
/// </summary>
public class UploadBlobOptions
{
    /// <summary>
    /// Whether to allow other clients to read the blob while the upload is still in progress.
    /// This can lead to improved performance for some backends, at the expense of losing atomicity guarantees.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// When an upload with partial reads allowed gets canceled, the blob might be deleted, which might cause race
    /// conditions if another upload with the same name started immediately before the cancellation. You can mitigate
    /// this by also setting <see cref="FailIfExists"/> to <see langword="true"/>.
    /// </remarks>
    public bool AllowPartialReads { get; set; }

    /// <summary>
    /// Whether to fail the upload if a blob with the same name already exists in the target bucket.
    /// Defaults to <see langword="false"/>.
    /// </summary>
    /// <remarks>
    /// Depending on the backend, the check might be performed either at the start of the upload, or at its completion.
    /// Most backends guarantee that the check is atomic with respect to other uploads, but this is not a requirement.
    /// </remarks>
    public bool FailIfExists { get; set; }

    /// <summary>
    /// The MIME content type to assign to the uploaded blob.
    /// Defaults to an empty string, in which case some backends might use a default value,
    /// such as <c>application/octet-stream</c>.
    /// </summary>
    /// <remarks>
    /// This property is intended for presentation purposes, and may be ignored by some backends.
    /// </remarks>
    public string ContentType { get; set; } = "";
}
