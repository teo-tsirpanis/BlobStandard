// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using BlobStandard.Models;
using BlobStandard.Utilities;
using Microsoft.Win32.SafeHandles;

namespace BlobStandard;

public partial class FileSystemBackend
{
    private static string GenerateTempFileName(string blobName) => $"{blobName}.{Guid.NewGuid():N}{BackendUtilities.TempBlobSuffix}";

    private sealed class UploaderStrategy : BlobUploaderStrategy
    {
        /// <summary>
        /// The name of the file to which data is currently being written.
        /// This can be either the user-specified blob name, or a temporary
        /// file if the upload is being performed atomically.
        /// </summary>
        private readonly string _blobName;

        /// <summary>
        /// If not null, the name of the user-specified file to which the
        /// temporary file will be moved to, once the upload is finalized.
        /// </summary>
        private readonly string? _finalDestinationName;

        private readonly bool _failIfExists;

        /// <summary>
        /// A handle to the file specified by <see cref="_blobName"/>.
        /// </summary>
        private readonly SafeFileHandle _handle;

        private long _offset;

        /// <summary>
        /// A reusable list of buffers that is populated with the memory segments of a multi-segment
        /// <see cref="ReadOnlySequence{T}"/>, and passed to <see cref="RandomAccess.WriteAsync(SafeFileHandle, IReadOnlyList{ReadOnlyMemory{byte}}, long, CancellationToken)"/>.
        /// </summary>
        private List<ReadOnlyMemory<byte>>? _buffers;

        private UploaderStrategy(string blobName, bool isAtomicWrite, bool failIfExists)
        {
            var fileMode = (isAtomicWrite, failIfExists) switch
            {
                (true, _) or (false, true) => FileMode.CreateNew,
                (false, false) => FileMode.Create,
            };
            var fileShare = isAtomicWrite ? FileShare.None : FileShare.Read;
            _blobName = isAtomicWrite ? GenerateTempFileName(blobName) : blobName;
            _finalDestinationName = isAtomicWrite ? blobName : null;
            _failIfExists = failIfExists;
            _handle = File.OpenHandle(_blobName, fileMode, FileAccess.Write, fileShare, FileOptions.Asynchronous);
        }

        public UploaderStrategy(string blobName, UploadBlobOptions? options)
            : this(blobName, options?.AllowPartialReads == false, options?.FailIfExists == true)
        {
        }

        protected override async ValueTask WriteBufferAsync(ReadOnlySequence<byte> buffer, bool isFinal, CancellationToken cancellationToken)
        {
            if (buffer.IsSingleSegment)
            {
                var first = buffer.First;
                await RandomAccess.WriteAsync(_handle, first, _offset, cancellationToken).ConfigureAwait(false);
                _offset += first.Length;
            }
            else
            {
                var buffers = _buffers ??= [];
                Debug.Assert(buffers.Count == 0, "Buffers should be cleared after each write.");
                foreach (var segment in buffer)
                {
                    buffers.Add(segment);
                }
                await RandomAccess.WriteAsync(_handle, buffers, _offset, cancellationToken).ConfigureAwait(false);
                buffers.Clear();
                _offset += buffer.Length;
            }

            if (isFinal)
            {
                // There's unfortunately no async version of FlushToDisk.
                RandomAccess.FlushToDisk(_handle);
                if (_finalDestinationName is not null)
                {
                    _handle.Dispose();
                    File.Move(_blobName, _finalDestinationName, overwrite: !_failIfExists);
                }
            }
        }

        protected override ValueTask AbortAsync()
        {
            _handle.Dispose();
            try
            {
                File.Delete(_blobName);
            }
            catch
            {
                // Best effort cleanup. If this fails, the temp file will be left on disk, but we don't want to throw an exception that would crash the process.
            }
            return default;
        }

        protected override void Dispose()
        {
            // The handle might have been disposed already, but that's fine since
            // disposing is idempotent.
            _handle.Dispose();
        }
    }
}
