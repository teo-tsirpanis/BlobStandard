// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Utilities;

/// <summary>
/// Signals non-throwing graceful cancellation across threads. This is a lightweight alternative to
/// <see cref="CancellationToken"/>.
/// </summary>
internal sealed class StopSignal
{
    public volatile bool ShouldStop = false;
}
