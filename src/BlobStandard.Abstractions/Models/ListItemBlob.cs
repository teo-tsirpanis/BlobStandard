// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Models;

/// <summary>
/// Represents a blob entry returned when listing the contents of a storage bucket.
/// </summary>
/// <seealso cref="IStorageBackend.ListBlobsAsync"/>
/// <seealso cref="IStorageBackend.ListBlobsByHierarchyAsync"/>
public class ListItemBlob : ListItemBase
{
    /// <summary>
    /// The name of the blob.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The size of the blob in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// The date and time when the blob was last modified, in UTC. If the storage backend
    /// does not provide this information, it will be set to <see cref="DateTime.MinValue"/>.
    /// </summary>
    public DateTime LastModified { get; set; }
}
