using EagleWeb.Common.IO.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web.IO.Streams
{
    public class EagleStreamServer : IEagleStream
    {
        public EagleStreamServer(string id, IEagleStreamServer server)
        {
            this.id = id;
            this.server = server;
        }

        private readonly string id;
        private readonly IEagleStreamServer server;

        public string Id => id;
        public IEagleStreamServer Server => server;
    }
}
