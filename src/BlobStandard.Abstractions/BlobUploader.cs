// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using System.IO.Pipelines;

namespace BlobStandard;

public abstract class BlobUploader
{
    public PipeWriter Writer { get; }

    protected BlobUploader(PipeWriter writer)
    {
        Writer = writer;
    }
}
