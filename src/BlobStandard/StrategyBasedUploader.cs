// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using System.IO.Pipelines;

namespace BlobStandard;

/// <summary>
/// Provides an implementation of <see cref="BlobUploader"/> that is backed by a
/// <see cref="BlobUploaderStrategy"/>.
/// </summary>
internal sealed class StrategyBasedUploader : BlobUploader
{
    private readonly PipeReader _reader;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public StrategyBasedUploader(Pipe pipe, BlobUploaderStrategy strategy) : base(pipe.Writer)
    {
        _reader = pipe.Reader;
        _ = strategy.RunAsync(pipe.Reader, _cancellationTokenSource.Token);
    }

    /// <inheritdoc/>
    public sealed override void Cancel()
    {
        _reader.CancelPendingRead();
        _cancellationTokenSource.Cancel();
    }
}
