// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using System.Diagnostics;
using System.Threading.Channels;

namespace BlobStandard.Utilities;

/// <summary>
/// Asynchronously enumerates items from a <see cref="ChannelReader{T}"/>.
/// In addition to <see cref="CancellationToken"/>, enumeration can also be canceled by checking a
/// <see cref="StopSignal"/>, which is signaled when the enumerator is disposed. This allows the
/// producer to release resources even when enumeration prematurely ends (have checked that calling
/// <see langword="break"/> in a <see langword="foreach"/> loop will call try-finally blocks in an
/// iterator method).
/// </summary>
internal sealed class ChannelReaderEnumerable<T>(Func<StopSignal, CancellationToken, ChannelReader<T>> readerFactory) : IAsyncEnumerable<T>
{
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        var stop = new StopSignal();
        var reader = readerFactory(stop, cancellationToken);
        return new Enumerator(reader, stop, cancellationToken);
    }

    internal sealed class Enumerator(ChannelReader<T> reader, StopSignal stop, CancellationToken cancellationToken) : IAsyncEnumerator<T>
    {
        private T _item = default!;

        public T Current => _item;

        public ValueTask DisposeAsync()
        {
            stop.ShouldStop = true;
            // Drain buffered items to unblock producer to reach a point where it reads the stop signal.
            while (reader.TryRead(out _)) { }
            return ValueTask.CompletedTask;
        }

        public async ValueTask<bool> MoveNextAsync()
        {
            if (!await reader.WaitToReadAsync(cancellationToken))
            {
                return false;
            }
            bool read = reader.TryRead(out _item!);
            Debug.Assert(read);
            return true;
        }
    }
}
