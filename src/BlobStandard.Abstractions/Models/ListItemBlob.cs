// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Models;

public class ListItemBlob : ListItemBase
{
    public required string Name { get; set; }

    public long Size { get; set; }

    public DateTime LastModified { get; set; }
}
