// Copyright © Theodore Tsirpanis and Contributors.
// SPDX-License-Identifier: MIT

using System.IO.Enumeration;
using System.Threading.Channels;
using BlobStandard.Models;
using BlobStandard.Utilities;

namespace BlobStandard;

public partial class FilesystemBackend
{
    private const int ListBufferSize = 16;

    private static readonly BoundedChannelOptions s_listChannelOptions = new(ListBufferSize)
    {
        SingleWriter = true,
        SingleReader = true,
        FullMode = BoundedChannelFullMode.Wait,
    };

    private static ChannelReaderEnumerable<ListItemBase> ListBlobsInternal(string directory, bool recurse)
    {
        return new ChannelReaderEnumerable<ListItemBase>((stop, cancellationToken) =>
        {
            var channel = Channel.CreateBounded<ListItemBase>(s_listChannelOptions);

            _ = EnumerateAsync(directory, recurse, channel, stop, cancellationToken);
            return channel;
        });

        static async Task EnumerateAsync(string directory, bool recurse, ChannelWriter<ListItemBase> writer, StopSignal stop, CancellationToken cancellationToken)
        {
            try
            {
                if (directory.Length == 0)
                {
                    await EnumerateRootAsync(recurse, writer, stop, cancellationToken);
                }
                else
                {
                    await EnumerateDirectoryAsync(directory, recurse, writer, stop, cancellationToken);
                }
                writer.Complete();
            }
            catch (Exception e)
            {
                writer.Complete(e);
            }
        }
    }

    private static async Task EnumerateRootAsync(bool recurse, ChannelWriter<ListItemBase> writer, StopSignal stop, CancellationToken cancellationToken = default)
    {
        // Windows does not have a single filesystem root, so enumerate all (ready) drives.
        if (OperatingSystem.IsWindows())
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (stop.ShouldStop)
                {
                    break;
                }
                cancellationToken.ThrowIfCancellationRequested();
                if (!drive.IsReady)
                {
                    continue;
                }
                if (recurse)
                {
                    await EnumerateDirectoryAsync(drive.Name, true, writer, stop, cancellationToken);
                }
                else
                {
                    await writer.WriteAsync(new ListItemPrefix { Name = drive.Name }, cancellationToken);
                }
            }
            return;
        }
        await EnumerateDirectoryAsync("/", recurse, writer, stop, cancellationToken);
    }

    private static async Task EnumerateDirectoryAsync(string directory, bool recurse, ChannelWriter<ListItemBase> writer, StopSignal stop, CancellationToken cancellationToken)
    {
        FilesystemBlobEnumerator enumerator;
        try
        {
            enumerator = new(directory, recurse);
        }
        catch (DirectoryNotFoundException)
        {
            // Return an empty list if the directory does not exist, for compatibility with object storage listing.
            return;
        }
        try
        {
            while (enumerator.MoveNext() && !stop.ShouldStop)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await writer.WriteAsync(enumerator.Current, cancellationToken);
            }
        }
        finally
        {
            enumerator.Dispose();
        }
    }

    private sealed class FilesystemBlobEnumerator(string directory, bool recurse)
        : FileSystemEnumerator<ListItemBase>(directory, recurse ? DefaultOptionsRecurse : DefaultOptions)
    {
        private static readonly EnumerationOptions DefaultOptions = new();

        private static readonly EnumerationOptions DefaultOptionsRecurse = new() { RecurseSubdirectories = true };

        protected override ListItemBase TransformEntry(ref FileSystemEntry entry)
        {
            if (entry.IsDirectory)
            {
                return new ListItemPrefix
                {
                    // Include a trailing separator, for compatibility with object storage listing.
                    Name = entry.ToSpecifiedFullPath() + Path.DirectorySeparatorChar,
                };
            }
            else
            {
                return new ListItemBlob
                {
                    Name = entry.ToSpecifiedFullPath(),
                    Size = entry.Length,
                    LastModified = entry.LastWriteTimeUtc.UtcDateTime,
                };
            }
        }

        protected override bool ShouldIncludeEntry(ref FileSystemEntry entry)
        {
            // Do not include directories when recursing, for compatibility with object storage listing.
            return !(recurse && entry.IsDirectory);
        }
    }
}
