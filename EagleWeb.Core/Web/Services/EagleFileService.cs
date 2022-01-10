using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web.Services
{
    /// <summary>
    /// Simply serves a file. Mostly just for testing.
    /// </summary>
    class EagleFileService : IEagleWebServerService
    {
        public EagleFileService(string filename, string mimeType)
        {
            this.filename = filename;
            this.mimeType = mimeType;
        }

        private string filename;
        private string mimeType;

        public async Task HandleRequest(HttpContext e)
        {
            //Open the file stream
            FileStream src;
            try
            {
                src = new FileStream(filename, FileMode.Open, FileAccess.Read);
            } catch
            {
                e.Response.StatusCode = 500;
                return;
            }

            //Copy
            e.Response.StatusCode = 200;
            e.Response.ContentType = mimeType;
            e.Response.ContentLength = src.Length;
            await src.CopyToAsync(e.Response.Body);

            //Close
            src.Close();
        }
    }
}
