// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using System.IO.Pipelines;

namespace BlobStandard;

/// <summary>
/// Represents an in-progress blob upload operation.
/// </summary>
/// <seealso cref="IStorageBackend.StartUploadBlobAsync"/>
public abstract class BlobUploader
{
    /// <summary>
    /// Gets the <see cref="PipeWriter"/> used to stream the blob's content to the storage backend.
    /// </summary>
    /// <remarks>
    /// You must call <see cref="PipeWriter.Complete"/> or <see cref="PipeWriter.CompleteAsync"/>
    /// on the writer to signal completion of the upload operation. Otherwise, the blob will not
    /// be visible in the storage backend.
    /// </remarks>
    public PipeWriter Writer { get; }

    /// <summary>
    /// Initializes a new instance of <see cref="BlobUploader"/> with the given <see cref="PipeWriter"/>.
    /// </summary>
    /// <param name="writer">The value of <see cref="Writer"/>.</param>
    protected BlobUploader(PipeWriter writer)
    {
        Writer = writer;
    }

    /// <summary>
    /// Cancels the upload operation.
    /// </summary>
    /// <remarks>
    /// This method must be called before completing <see cref="Writer"/>.
    /// </remarks>
    public abstract void Cancel();
}
