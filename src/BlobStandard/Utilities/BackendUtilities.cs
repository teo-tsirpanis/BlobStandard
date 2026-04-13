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
}
