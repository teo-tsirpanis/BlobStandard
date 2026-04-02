// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using System.IO.Pipelines;

namespace BlobStandard.Models;

/// <summary>
/// Represents the response returned when downloading a blob, including its content stream and details.
/// </summary>
public class BlobDownloadResponse
{
    /// <summary>
    /// The <see cref="PipeReader"/> from which the blob's content can be read.
    /// </summary>
    public required PipeReader Content { get; set; } = null!;

    /// <summary>
    /// Information about the downloaded blob.
    /// </summary>
    public required BlobDetails Details { get; set; } = null!;
}
