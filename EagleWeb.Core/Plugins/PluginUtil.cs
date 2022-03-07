using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Plugins
{
    public static class PluginUtil
    {
        public static List<ZipArchiveEntry> FindEntries(this ZipArchive archive, string prefix)
        {
            List<ZipArchiveEntry> results = new List<ZipArchiveEntry>();
            foreach (var e in archive.Entries)
            {
                if (e.FullName.StartsWith(prefix))
                    results.Add(e);
            }
            return results;
        }

        public static Stream OpenFixed(this ZipArchiveEntry entry)
        {
            return new WrappedEntry(entry);
        }

        public static string IdentifyPlatform()
        {
            //Determine os;
            string os;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                os = "win";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                os = "linux";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                os = "osx";
            else
                throw new Exception("Could not determine current OS!");

            //Determine arch
            string arch;
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X64: arch = "x64"; break;
                case Architecture.X86: arch = "x86"; break;
                case Architecture.Arm: arch = "arm"; break;
                case Architecture.Arm64: arch = "arm64"; break;
                default: throw new Exception("Could not determine current architecture!");
            }

            return os + "-" + arch;
        }

        /* Creates a wrapped stream around a ZipArchiveEntry to provide length...because for some reason it doesn't? */

        class WrappedEntry : Stream
        {
            public WrappedEntry(ZipArchiveEntry entry)
            {
                this.entry = entry;
                entryStream = entry.Open();
            }

            private ZipArchiveEntry entry;
            private Stream entryStream;

            public override bool CanRead => entryStream.CanRead;

            public override bool CanSeek => entryStream.CanSeek;

            public override bool CanWrite => entryStream.CanWrite;

            public override long Length => entry.Length; /* Main difference */

            public override long Position { get => entryStream.Position; set => entryStream.Position = value; }

            public override void Flush()
            {
                entryStream.Flush();
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return entryStream.Read(buffer, offset, count);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return entryStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                entryStream.SetLength(value);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                entryStream.Write(buffer, offset, count);
            }

            public override void Close()
            {
                base.Close();
                entryStream.Close();
            }
        }
    }
}
