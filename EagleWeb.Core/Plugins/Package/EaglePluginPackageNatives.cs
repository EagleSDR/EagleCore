using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace EagleWeb.Core.Plugins.Package
{
    class EaglePluginPackageNatives
    {
        public EaglePluginPackageNatives(ZipArchive archive, string prefix)
        {
            //Set
            this.archive = archive;

            //Find entries
            entries = archive.FindEntries(prefix);
        }

        private readonly ZipArchive archive;
        private readonly List<ZipArchiveEntry> entries;

        public void Unpack(Action<string, Stream> callback)
        {
            foreach (var e in entries)
            {
                using (Stream file = e.OpenFixed())
                    callback(e.Name, file);
            }
        }
    }
}
