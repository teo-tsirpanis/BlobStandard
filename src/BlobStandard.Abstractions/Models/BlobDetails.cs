// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Models;

public class BlobDetails
{
    public long Size { get; set; }

    public DateTime LastModified { get; set; }

    public string ContentType { get; set; } = "";
}
