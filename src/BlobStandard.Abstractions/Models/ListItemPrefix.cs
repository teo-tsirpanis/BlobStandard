// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Models;

/// <summary>
/// Represents a common prefix (virtual directory) entry returned when listing
/// a storage bucket with a hierarchical delimiter.
/// </summary>
/// <seealso cref="IStorageBackend.ListBlobsByHierarchyAsync"/>
public class ListItemPrefix : ListItemBase
{
    /// <summary>
    /// The name of the common prefix (virtual directory).
    /// </summary>
    public required string Name { get; set; }
}
