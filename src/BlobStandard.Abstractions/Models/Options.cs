// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Models;

public class DeleteBlobOptions;

public class DownloadBlobOptions
{
    public long RangeStart { get; set; }

    public long? RangeCount { get; set; }
}

public class GetBlobDetailsOptions;

public class ListBlobsOptions;

public class UploadBlobOptions
{
    public bool AllowPartialReads { get; set; }

    public bool FailIfExists { get; set; }

    public string ContentType { get; set; } = "";
}
