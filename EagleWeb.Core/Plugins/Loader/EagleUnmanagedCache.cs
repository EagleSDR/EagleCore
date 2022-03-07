using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace EagleWeb.Core.Plugins.Loader
{
    class EagleUnmanagedCache
    {
        public EagleUnmanagedCache(IEagleLogger logger, DirectoryInfo dir)
        {
            this.logger = logger;
            this.dir = dir;
        }

        private readonly IEagleLogger logger;
        private readonly DirectoryInfo dir;
        private readonly List<string> files = new List<string>();

        public void Activate()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!SetDllDirectoryW(dir.FullName))
                    throw new Exception("Failed to activate unmanaged cache: SetDllDirectoryW returned false.");
            } else
            {
                throw new PlatformNotSupportedException();
            }
        }

        public void CopyTo(string name, Stream source)
        {
            //Create full filename
            string filename = dir.FullName + Path.DirectorySeparatorChar + name;

            //If the file exists already in the cache, skip
            if (files.Contains(name))
                return;

            //Copy file
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Create))
                    source.CopyTo(fs);
            } catch (IOException)
            {
                logger.Log(Common.EagleLogLevel.WARN, "EagleUnmanagedCache", $"Failed to copy unmanaged DLL {name} because the destination file is in use.");
            }

            //Add to cache
            files.Add(name);
        }

        public bool TryLocateFile(string name, out string fullFilename)
        {
            //Create the real filename
            fullFilename = dir.FullName + Path.DirectorySeparatorChar + name;

            //Make sure it exists
            return File.Exists(fullFilename);
        }

        public void Clean()
        {
            //Query a list of files in the directory
            FileInfo[] existingFiles = dir.GetFiles();

            //Scan files and delete anything that is not in the cache
            foreach (var f in existingFiles)
            {
                //Check if it's in the cache
                if (files.Contains(f.Name))
                    continue;

                //Delete
                f.Delete();
            }
        }

        /* Native calls for activate */

        [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern bool SetDllDirectoryW(string lpPathName);
    }
}
