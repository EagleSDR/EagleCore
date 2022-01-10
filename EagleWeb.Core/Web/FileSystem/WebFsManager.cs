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
    public class WebFsManager : EagleObject
    {
        public WebFsManager(EagleContext ctx, DirectoryInfo root) : base(ctx)
        {
            this.root = root;
        }

        private readonly DirectoryInfo root;
        private Dictionary<string, WebFsFileStreamImpl> streams = new Dictionary<string, WebFsFileStreamImpl>();

        private IEaglePortApi portFileOpen;

        protected override void ConfigureObject(IEagleObjectConfigureContext context)
        {
            base.ConfigureObject(context);
            portFileOpen = context.CreatePortApi("OpenFile")
                .Bind(ApiHandleFileOpen);
        }

        public string UserOpenFile(IEagleAccount account, string path, bool writing)
        {
            //Determine the whole path
            FileInfo file = new FileInfo(Path.Combine(GetUserDirectory(account).FullName, path));

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

        /* API */

        private JObject ApiHandleFileOpen(IEagleClient client, JObject payload)
        {
            //Get arguments
            string filename = payload.GetString("filename");
            bool write = payload.GetBool("write");

            //Open
            string token = UserOpenFile(client.Account, filename, write);

            //Make response
            JObject response = new JObject();
            response["token"] = token;
            return response;
        }
    }
}
