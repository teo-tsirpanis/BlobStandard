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
        // Don't use the cancellation token for reads; we rely on CancelPendingRead to cancel while
        // waiting for more data, without throwing an exception.
        _ = strategy.RunAsync(pipe.Reader, CancellationToken.None, _cancellationTokenSource.Token);
    }

    /// <inheritdoc/>
    public sealed override void Cancel()
    {
        _reader.CancelPendingRead();
        _cancellationTokenSource.Cancel();
    }
}
