using EagleWeb.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web
{
    class EagleWebServer
    {
        public EagleWebServer(EagleContext ctx, int port)
        {
            this.ctx = ctx;
            this.port = port;
        }

        private readonly EagleContext ctx;
        private readonly int port;

        private Dictionary<string, IEagleWebServerService> services = new Dictionary<string, IEagleWebServerService>();

        public Task RunAsync()
        {
            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    IPAddress addr = IPAddress.Any;
                    options.Listen(addr, port);

                })
                .UseStartup<EagleWebServer>()
                .Configure(Configure)
                .Build();

            return host.RunAsync();
        }

        public void RegisterService(string path, IEagleWebServerService service)
        {
            services[path] = service;
        }

        private void Configure(IApplicationBuilder app)
        {
            app.UseWebSockets();
            app.Run(HandleRequest);
        }

        private async Task HandleRequest(HttpContext e)
        {
            ctx.Log(EagleLogLevel.DEBUG, "EagleWebServer", $"Got {e.Request.Method} from {e.Request.HttpContext.Connection.RemoteIpAddress} -> {e.Request.Path}");
            try
            {
                if (services.TryGetValue(e.Request.Path, out IEagleWebServerService service))
                    await service.HandleRequest(e);
                else
                    e.Response.StatusCode = 404;
            } catch (Exception ex)
            {
                ctx.Log(EagleLogLevel.WARN, "EagleWebServer", $"Unhandled Exception at ({e.Request.Path}): {ex.Message} {ex.StackTrace}");
                e.Response.StatusCode = 500;
            }
        }
    }
}
