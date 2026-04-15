// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

namespace BlobStandard.Utilities;

internal static class BackendUtilities
{
    /// <summary>
    /// A reserved suffix for temporary blobs that BlobStandard might create during uploads.
    /// </summary>
    /// <remarks>
    /// In order to avoid conflicts, storage backends must reject all operations on user-specified
    /// blob names that end with this string, and exclude such blobs from listing results.
    /// </remarks>
    public const string TempBlobSuffix = ".blobstd.tmp";

    public static void ValidateBlobName(string blobName)
    {
        ArgumentException.ThrowIfNullOrEmpty(blobName);
        if (blobName.EndsWith(TempBlobSuffix, StringComparison.Ordinal))
            throw new ArgumentException($"Blob names cannot end with the reserved suffix '{TempBlobSuffix}'.", nameof(blobName));
    }

    extension(Stream stream)
    {

        public int GetCopyBufferSize()
        {
            // https://github.com/dotnet/runtime/blob/25e2d7fbb1bc5f9b3fe2505671e2902d8fb40b5a/src/libraries/System.Private.CoreLib/src/System/IO/Stream.cs#L118-L154

            // This value was originally picked to be the largest multiple of 4096 that is still smaller than the large object heap threshold (85K).
            // The CopyTo{Async} buffer is short-lived and is likely to be collected at Gen0, and it offers a significant improvement in Copy
            // performance.  Since then, the base implementations of CopyTo{Async} have been updated to use ArrayPool, which will end up rounding
            // this size up to the next power of two (131,072), which will by default be on the large object heap.  However, most of the time
            // the buffer should be pooled, the LOH threshold is now configurable and thus may be different than 85K, and there are measurable
            // benefits to using the larger buffer size.  So, for now, this value remains.
            const int DefaultCopyBufferSize = 81920;

            int bufferSize = DefaultCopyBufferSize;

            if (stream.CanSeek)
            {
                long length = stream.Length;
                long position = stream.Position;
                if (length <= position) // Handles negative overflows
                {
                    // There are no bytes left in the stream to copy.
                    // However, because CopyTo{Async} is virtual, we need to
                    // ensure that any override is still invoked to provide its
                    // own validation, so we use the smallest legal buffer size here.
                    bufferSize = 1;
                }
                else
                {
                    long remaining = length - position;
                    if (remaining > 0)
                    {
                        // In the case of a positive overflow, stick to the default size
                        bufferSize = (int)Math.Min(bufferSize, remaining);
                    }
                }
            }

            return bufferSize;
        }

    }
}
