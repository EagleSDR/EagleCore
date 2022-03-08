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
    class EagleFixedAssetService : IEagleWebServerService
    {
        public EagleFixedAssetService(IEaglePluginPackageAsset asset)
        {
            //Read the entire file into memory
            using (Stream file = asset.Open())
            {
                data = new byte[file.Length];
                file.Read(data, 0, data.Length);
            }

            //Determine MIME type
            mime = MimeTypeLookup.GetMimeType(asset.FileName);
        }

        private readonly byte[] data;
        private readonly string mime;

        public Task HandleRequest(HttpContext e)
        {
            //Set
            e.Response.StatusCode = 200;
            e.Response.ContentType = mime;
            e.Response.ContentLength = data.Length;

            //Copy data
            return e.Response.Body.WriteAsync(data, 0, data.Length);
        }
    }
}
