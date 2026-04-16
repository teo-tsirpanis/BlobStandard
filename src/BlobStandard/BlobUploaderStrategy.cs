// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.IO.Pipelines;

namespace BlobStandard;

/// <summary>
/// Provides a standardized pattern to implement <see cref="BlobUploader"/>s.
/// </summary>
/// <remarks>
/// The loop will read data written to the <see cref="BlobUploader.Writer"/>, and call the following methods in order:
/// <list type="number">
/// <item><description>Zero or more calls to <see cref="WriteBufferAsync"/>, with the <c>isFinal</c> parameter set to <see langword="false"/>.</description></item>
/// <item><description>If <see cref="BlobUploader.Writer"/> completes successfully, one call to <see cref="WriteBufferAsync"/>, with the <c>isFinal</c> parameter set to <see langword="true"/>.</description></item>
/// <item><description>If <see cref="BlobUploader.Writer"/> completes with an exception or <see cref="BlobUploader.Cancel"/> is called, one call to <see cref="AbortAsync"/>.</description></item>
/// <item><description>Finally, one call to <see cref="Dispose"/>.</description></item>
/// </list>
/// </remarks>
internal abstract class BlobUploaderStrategy
{
    /// <summary>
    /// Gets called to write a buffer of data to the blob.
    /// </summary>
    /// <param name="buffer">The buffer of data to write.</param>
    /// <param name="isFinal">Indicates whether this is the final buffer.</param>
    /// <param name="cancellationToken">Used to cancel the operation.</param>
    /// <remarks>
    /// <para>
    /// The entirety of <paramref name="buffer"/> must be written before this method returns; partial writes are not allowed.
    /// </para>
    /// <para>
    /// If <paramref name="isFinal"/> is set to <see langword="true"/>, the implementation must finalize the upload
    /// after writing the buffer, and no further calls to <see cref="WriteBufferAsync"/> will be made.
    /// </para>
    /// </remarks>
    protected abstract ValueTask WriteBufferAsync(ReadOnlySequence<byte> buffer, bool isFinal, CancellationToken cancellationToken);

    /// <summary>
    /// Gets called when the upload operation is aborted, either due to an exception,
    /// or because the upload was canceled.
    /// </summary>
    protected abstract ValueTask AbortAsync();

    /// <summary>
    /// Gets called after the upload loop has exited, regardless of whether it was successful or not.
    /// This can be overridden to dispose unmanaged resources, such as file handles.
    /// </summary>
    protected virtual void Dispose() { }

    public Task RunAsync(PipeReader reader, CancellationToken cancellationToken = default)
    {
        return RunAsync(reader, cancellationToken, cancellationToken);
    }

    public async Task RunAsync(PipeReader reader, CancellationToken cancellationTokenForRead, CancellationToken cancellationTokenForWrite)
    {
        try
        {
            bool isCanceled = false;
            while (true)
            {
                ReadResult readResult = await reader.ReadAsync(cancellationTokenForRead).ConfigureAwait(false);
                try
                {
                    if (readResult.IsCanceled)
                    {
                        isCanceled = true;
                        break;
                    }
                    await WriteBufferAsync(readResult.Buffer, readResult.IsCompleted, cancellationTokenForWrite).ConfigureAwait(false);
                    if (readResult.IsCompleted)
                    {
                        break;
                    }
                }
                finally
                {
                    reader.AdvanceTo(readResult.Buffer.End);
                }
            }

            if (isCanceled)
            {
                await AbortAsync().ConfigureAwait(false);
                await reader.CompleteAsync(new OperationCanceledException()).ConfigureAwait(false);
            }
            else
            {
                await reader.CompleteAsync().ConfigureAwait(false);
            }
        }
        catch (Exception e)
        {
            await AbortAsync().ConfigureAwait(false);
            await reader.CompleteAsync(e).ConfigureAwait(false);
        }
        finally
        {
            Dispose();
        }
    }
}
