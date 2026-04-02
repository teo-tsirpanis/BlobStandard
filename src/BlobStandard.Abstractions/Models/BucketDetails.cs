// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Models;

/// <summary>
/// Contains details about a storage bucket.
/// </summary>
public class BucketDetails
{
    /// <summary>
    /// Gets or sets the name of the bucket.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the geographic location of the bucket, or an empty string if not specified.
    /// </summary>
    public string Location { get; set; } = "";
}
