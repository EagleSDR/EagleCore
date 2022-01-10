using EagleWeb.Common.IO.Streams;
using EagleWeb.Core.Auth;
using EagleWeb.Core.Web.WS;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web.IO.Streams
{
    public class EagleStreamService : EagleWsConnectionService
    {
        public EagleStreamService(EagleContext ctx) : base(ctx, (EagleWsConnectionService ctx, EagleAccount account) => new EagleStreamClient(ctx as EagleStreamService, account))
        {
        }

        private Dictionary<string, EagleStreamServer> servers = new Dictionary<string, EagleStreamServer>();

        public bool TryGetServer(string id, out EagleStreamServer server)
        {
            return servers.TryGetValue(id, out server);
        }

        public EagleStreamServer CreateServer(string id, IEagleStreamServer server)
        {
            EagleStreamServer s = new EagleStreamServer(id, server);
            servers.Add(id, s);
            return s;
        }
    }
}
