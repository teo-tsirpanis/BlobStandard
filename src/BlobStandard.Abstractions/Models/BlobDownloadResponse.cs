// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using System.IO.Pipelines;

namespace BlobStandard.Models;

public class BlobDownloadResponse
{
    public required PipeReader Content { get; set; } = null!;

    public required BlobDetails Details { get; set; } = null!;
}
