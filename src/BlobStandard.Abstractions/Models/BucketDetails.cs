// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Models;

public class BucketDetails
{
    public required string Name { get; set; }

    public string Location { get; set; } = "";
}
