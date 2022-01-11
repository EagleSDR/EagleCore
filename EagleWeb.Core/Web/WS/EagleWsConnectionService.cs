using EagleWeb.Core.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebSockets;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web.WS
{
    public delegate EagleBaseConnection EagleWsConnectionService_CreateConnection(EagleWsConnectionService ctx, EagleAccount account);

    public class EagleWsConnectionService : EagleWsConnectionService2
    {
        public EagleWsConnectionService(EagleContext ctx, EagleWsConnectionService_CreateConnection constructor) : base(ctx)
        {
            this.ctx = ctx;
            this.constructor = constructor;
        }

        private readonly EagleContext ctx;
        private readonly EagleWsConnectionService_CreateConnection constructor;

        protected override bool CreateConnection(HttpContext e, EagleAccount account, out EagleBaseConnection connection)
        {
            connection = constructor(this, account);
            return true;
        }
    }
}
