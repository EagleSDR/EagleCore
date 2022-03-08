using EagleWeb.Core.Plugins;
using EagleWeb.Core.Plugins.Package;
using EagleWeb.Core.Web.Util;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web.Services
{
    class EagleAssetService : IEagleWebServerService
    {
        public EagleAssetService(EaglePluginManager manager)
        {
            this.manager = manager;
        }

        private EaglePluginManager manager;

        public async Task HandleRequest(HttpContext e)
        {
            //Get the requested hash
            string hash;
            if (!e.Request.Query.TryGetString("hash", out hash))
            {
                e.Response.StatusCode = 400;
                return;
            }

            //Validate that this is a valid hash
            if (!ValidateHash(hash))
            {
                e.Response.StatusCode = 400;
                return;
            }

            //Attempt to get it
            IEaglePluginPackageAsset asset;
            if (!manager.TryGetAsset(hash, out asset))
            {
                e.Response.StatusCode = 404;
                return;
            }

            //Lookup MIME type
            string mime = MimeTypeLookup.GetMimeType(asset.FileName);

            //Copy
            using (Stream src = asset.Open())
            {
                e.Response.StatusCode = 200;
                e.Response.ContentLength = src.Length;
                e.Response.ContentType = mime;
                await src.CopyToAsync(e.Response.Body);
            }
        }

        private bool ValidateHash(string hash)
        {
            //Make sure the length is correct
            if (hash.Length != 64)
                return false;

            //Make sure all characters are valid
            bool valid = true;
            foreach (var c in hash)
            {
                valid = valid && (
                    c == '0' ||
                    c == '1' ||
                    c == '2' ||
                    c == '3' ||
                    c == '4' ||
                    c == '5' ||
                    c == '6' ||
                    c == '7' ||
                    c == '8' ||
                    c == '9' ||
                    c == 'A' ||
                    c == 'B' ||
                    c == 'C' ||
                    c == 'D' ||
                    c == 'E' ||
                    c == 'F'
                    );
            }
            return valid;
        }
    }
}
