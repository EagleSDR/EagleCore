using EagleWeb.Package.Data;
using EagleWeb.Package.Manifest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace EagleWeb.Package
{
    public class EaglePackageReader : IDisposable
    {
        public EaglePackageReader(Stream file)
        {
            //Open file
            zip = new ZipArchive(file, ZipArchiveMode.Read, true);

            //Load manifest
            manifest = ReadManifest();
        }

        private ZipArchive zip;
        private EagleManifest manifest;

        public EagleManifest Manifest => manifest;
        public string Id => $"{manifest.developer_name}.{manifest.plugin_name}";
        public Version Version => new Version(manifest.version_major, manifest.version_minor, manifest.version_build);

        public void CopyLibs(string platform, DirectoryInfo outputDir)
        {
            CopyLibsFrom($"native/{platform}/lib/", outputDir);
            CopyLibsFrom($"native/{platform}/bin/", outputDir);
            CopyLibsFrom($"managed/", outputDir);
        }

        private void CopyLibsFrom(string prefix, DirectoryInfo outputDir)
        {
            foreach (var e in zip.Entries)
            {
                //Skip items outside of what we're looking for
                if (!e.FullName.StartsWith(prefix))
                    continue;

                //Get the name and the output
                string name = e.FullName.Substring(prefix.Length);
                string dst = outputDir.FullName + Path.DirectorySeparatorChar + name;

                //Make sure this isn't a subdirectory
                if (name.Contains('/') || name.Contains('\\'))
                    continue;

                //Copy to disk
                using (Stream a = e.Open())
                using (Stream b = new FileStream(dst, FileMode.Create, FileAccess.Write))
                    a.CopyTo(b);
            }
        }

        private bool CompareFiles(Stream a, Stream b, int bufferSize = 4096)
        {
            //Go through the file, comparing all bytes
            byte[] bufferA = new byte[bufferSize];
            byte[] bufferB = new byte[bufferSize];
            int read;
            do
            {
                //Read from both files
                read = a.Read(bufferA, 0, bufferA.Length);
                if (b.Read(bufferB, 0, bufferB.Length) != read)
                    return false;

                //Compare
                for (int i = 0; i < read; i++)
                {
                    if (bufferA[i] != bufferB[i])
                        return false;
                }
            } while (read != 0);

            return true;
        }

        private EagleManifest ReadManifest()
        {
            string file;
            using (Stream src = zip.GetEntry("eagle-manifest.json").Open())
            using (StreamReader reader = new StreamReader(src))
                file = reader.ReadToEnd();
            return JsonConvert.DeserializeObject<EagleManifest>(file);
        }

        public Dictionary<string, string> CopyWebAssets(DirectoryInfo outputDir)
        {
            //Create hash map
            Dictionary<string, string> hashes = new Dictionary<string, string>();

            //Enumerate
            foreach (var e in zip.Entries)
            {
                //Make sure this starts with our prefix
                if (!e.FullName.StartsWith("assets/"))
                    continue;

                //Get name
                string name = e.FullName.Substring("assets/".Length);
                if (name.Length == 0 || e.Length == 0)
                    continue;

                //Read to file and hash it in the process
                string hash;
                string tempFilename = outputDir.FullName + Path.DirectorySeparatorChar + "file.tmp";
                using (SHA256 sha = SHA256.Create())
                using (Stream dst = new FileStream(tempFilename, FileMode.Create))
                using (Stream src = e.Open())
                {
                    //Copy
                    byte[] buffer = new byte[4096];
                    int read;
                    do
                    {
                        read = src.Read(buffer, 0, buffer.Length);
                        dst.Write(buffer, 0, read);
                        if (read == buffer.Length)
                            sha.TransformBlock(buffer, 0, read, buffer, 0);
                        else
                            sha.TransformFinalBlock(buffer, 0, read);
                    } while (read == buffer.Length);

                    //Get the hash
                    hash = BitConverter.ToString(sha.Hash).Replace("-", "");
                }

                //Determine output filename from the hash
                string outputFilename = outputDir.FullName + Path.DirectorySeparatorChar + hash + ".asset";

                //Delete existing output, if nay
                if (File.Exists(outputFilename))
                    File.Delete(outputFilename);

                //Move
                File.Move(tempFilename, outputFilename);

                //Create info
                string info = JsonConvert.SerializeObject(new EaglePluginAssetInfo
                {
                    original_name = e.Name
                });

                //Write file info
                string outputInfoFilename = outputFilename + "info";
                if (File.Exists(outputInfoFilename))
                    File.Delete(outputInfoFilename);
                File.WriteAllText(outputInfoFilename, info);

                //Register
                hashes.Add(name, hash);
            }

            return hashes;
        }

        public void Dispose()
        {
            zip.Dispose();
        }
    }
}
