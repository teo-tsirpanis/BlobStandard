// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Models;

/// <summary>
/// Contains basic details about a blob.
/// </summary>
public class BlobDetails
{
    /// <summary>
    /// The blob's size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// The date and time when the blob was last modified, in UTC. If the storage backend
    /// does not provide this information, it will be set to <see cref="DateTime.MinValue"/>.
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// The MIME content type of the blob, or an empty string if not defined.
    /// </summary>
    public string ContentType { get; set; } = "";
}
