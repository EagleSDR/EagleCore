using EagleWeb.Common;
using EagleWeb.Common.Auth;
using EagleWeb.Common.IO;
using EagleWeb.Common.IO.FileSystem;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.NetObjects.IO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace EagleWeb.Core.Web.FileSystem
{
    class WebFsManager : EagleObject
    {
        public WebFsManager(IEagleObjectContext context, EagleContext ctx, DirectoryInfo root) : base(context)
        {
            this.root = root;

            //Configure
            portFileOpen = context.CreatePortApi("OpenFile")
                .Bind(ApiHandleFileOpen);
            portDirInfo = context.CreatePortApi("QueryDirectory")
               .Bind(ApiHandleListDirectory);
            portDirCreate = context.CreatePortApi("CreateDirectory")
               .Bind(ApiHandleCreateDirectory);
            portDelete = context.CreatePortApi("Delete")
               .Bind(ApiHandleDelete);
            portQueryQuota = context.CreatePortApi("QueryQuota")
               .Bind(ApiHandleQueryQuota);
        }

        private readonly DirectoryInfo root;
        private Dictionary<string, WebFsFileStreamImpl> streams = new Dictionary<string, WebFsFileStreamImpl>();

        private IEaglePortApi portFileOpen;
        private IEaglePortApi portDirInfo;
        private IEaglePortApi portDirCreate;
        private IEaglePortApi portDelete;
        private IEaglePortApi portQueryQuota;

        public string UserOpenFile(IEagleAccount account, string path, bool writing)
        {
            //Determine the whole path
            FileInfo file = new FileInfo(ResolveUserPath(account, path));

            //Ensure this is within the user's directory
            EnsureUserDirectory(file, account);

            //Generate a random token and insert a placeholder in the tokens
            string token;
            lock (streams)
            {
                do
                {
                    token = "eagle://" + EagleWebHelpers.GenerateToken(32);
                } while (streams.ContainsKey(token));
                streams.Add(token, null);
            }

            //Open the file and set the token
            WebFsFileStreamImpl stream = null;
            try
            {
                stream = new WebFsFileStreamImpl(this, token, file.FullName, account, writing);
            }
            finally
            {
                //Remove reservation or set it
                lock (streams)
                {
                    if (stream == null)
                        streams.Remove(token);
                    else
                        streams[token] = stream;
                }

                //Log
                if (stream == null)
                    Log(EagleLogLevel.ERROR, "UserOpenFile", $"User \"{account.Username}\" attempted to open file \"{file.FullName}\" but FAILED.");
                else
                    Log(EagleLogLevel.INFO, "UserOpenFile", $"User \"{account.Username}\" opened file \"{file.FullName}\"; token={token}");
            }

            return token;
        }

        private DirectoryInfo GetUserDirectory(IEagleAccount account)
        {
            return root.CreateSubdirectory(account.Username);
        }

        private void EnsureUserDirectory(FileSystemInfo file, IEagleAccount account)
        {
            if (!file.FullName.StartsWith(GetUserDirectory(account).FullName))
            {
                Log(EagleLogLevel.WARN, "EnsureUserDirectory", $"Account \"{account.Username}\" attempted to access out-of-scope file: \"{file.FullName}\"! Rejecting...");
                throw new WebFsOutOfScopeException();
            }
        }

        public WebFsFileStream ResolveFileTokenImpl(string token)
        {
            lock (streams)
            {
                if (streams.ContainsKey(token))
                    return streams[token];
            }
            throw new WebFsTokenInvalidException();
        }

        public void NotifyFileClosed(string token)
        {
            lock(streams)
                streams.Remove(token);
        }

        private JObject SerializeDirectoryInfo(IEagleAccount account, DirectoryInfo dir)
        {
            //Query
            var subdirs = dir.GetDirectories();
            var files = dir.GetFiles();

            //Serialize file list
            JArray resultFiles = new JArray();
            foreach (var f in files)
            {
                resultFiles.Add(new JObject()
                {
                    {"name", f.Name },
                    {"size", f.Length / 1024.0 }, // Convert to KB so JavaScript doesn't start to have floating point issues with BIG files
                    {"last_modified", f.LastWriteTimeUtc }
                });
            }

            //Serialize the directory list
            JArray resultSubdirs = new JArray();
            foreach (var d in subdirs)
            {
                resultSubdirs.Add(new JObject()
                {
                    {"name", d.Name },
                    {"last_modified", d.LastWriteTimeUtc }
                });
            }

            //Determine the tree
            JArray path = new JArray();
            DirectoryInfo root = GetUserDirectory(account);
            DirectoryInfo cursor = dir;
            while (cursor.FullName.TrimEnd(Path.DirectorySeparatorChar) != root.FullName.TrimEnd(Path.DirectorySeparatorChar))
            {
                path.Insert(0, cursor.Name);
                cursor = cursor.Parent;
            }

            //Create response
            JObject response = new JObject();
            response["files"] = resultFiles;
            response["subdirectories"] = resultSubdirs;
            response["name"] = dir.Name;
            response["path"] = path;
            response["volume"] = account.Username;
            return response;
        }

        private string ResolveUserPath(IEagleAccount account, string path)
        {
            return GetUserDirectory(account).FullName + path.Replace('/', Path.DirectorySeparatorChar);
        }

        /* API */

        private JObject ApiHandleFileOpen(IEagleAccount account, JObject payload)
        {
            //Get arguments
            string filename = payload.GetString("filename");
            bool write = payload.GetBool("write");

            //Open
            string token = UserOpenFile(account, filename, write);

            //Make response
            JObject response = new JObject();
            response["token"] = token;
            return response;
        }

        private JObject ApiHandleListDirectory(IEagleAccount account, JObject payload)
        {
            //Get arguments
            DirectoryInfo dir = new DirectoryInfo(ResolveUserPath(account, payload.GetString("path")));

            //Ensure this is within the user's directory
            EnsureUserDirectory(dir, account);

            //Make sure it exists
            if (!dir.Exists)
                dir.Create();

            //Serialize
            return SerializeDirectoryInfo(account, dir);
        }

        private JObject ApiHandleDelete(IEagleAccount account, JObject payload)
        {
            //Get arguments
            string path = ResolveUserPath(account, payload.GetString("path"));

            //Determine if this is a directory or a file
            if (!File.Exists(path) || !Directory.Exists(path))
            {
                //Does not exist
                throw new Exception("File or directory does not exist.");
            }
            else if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                //As directory
                DirectoryInfo dir = new DirectoryInfo(path);

                //Ensure this is within the user's directory
                EnsureUserDirectory(dir, account);

                //Delete
                dir.Delete(true);
            } else
            {
                //As file
                FileInfo file = new FileInfo(path);

                //Ensure this is within the user's directory
                EnsureUserDirectory(file, account);

                //Delete
                file.Delete();
            }

            //Return empty response
            return new JObject();
        }

        private JObject ApiHandleCreateDirectory(IEagleAccount account, JObject payload)
        {
            //Get arguments
            DirectoryInfo dir = new DirectoryInfo(ResolveUserPath(account, payload.GetString("path")));

            //Ensure this is within the user's directory
            EnsureUserDirectory(dir, account);

            //If it does not exist, create
            if (!dir.Exists)
                dir.Create();

            //Return query
            return SerializeDirectoryInfo(account, dir);
        }

        private JObject ApiHandleQueryQuota(IEagleAccount account, JObject payload)
        {
            //Get the volume
            DriveInfo drive = new DriveInfo(GetUserDirectory(account).Root.FullName);

            //Create response
            JObject response = new JObject();
            response["capacity"] = drive.TotalSize;
            response["free"] = drive.AvailableFreeSpace;
            return response;
        }
    }
}
