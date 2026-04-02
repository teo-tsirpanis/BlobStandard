// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Models;

/// <summary>
/// The base class for items returned when listing the contents of a storage bucket.
/// The concrete types are <see cref="ListItemBlob"/> and <see cref="ListItemPrefix"/>.
/// </summary>
/// <seealso cref="IStorageBackend.ListBlobsByHierarchyAsync"/>
public abstract class ListItemBase
{
    private protected ListItemBase() { }
}
