using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Core.NetObjects.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects.Ports
{
    class EaglePortApi : EaglePortApiBase, IEaglePortApi
    {
        public EaglePortApi(EagleNetObjectInstance ctx, string name) : base(ctx, name)
        {
            handler = (IEagleClient client, JObject message) =>
            {
                throw new Exception($"Plugin Error: This API ({name}) is not implemented by the plugin.");
            };
        }

        private IEaglePortApi_Handler handler;

        public override EagleNetObjectPortType PortType => EagleNetObjectPortType.PORT_API;

        public IEaglePortApi Bind(IEaglePortApi_Handler handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");
            this.handler = handler;
            return this;
        }

        protected override void CreateExtra(JObject extra)
        {
            
        }

        public override void OnClientConnect(IEagleTarget target)
        {
            
        }

        protected override JObject OnClientApiCommand(IEagleClient client, JObject message)
        {
            //Validate that we have permission
            EnsureClientPermission(client);

            //Run handler
            return handler(client, message);
        }

        public IEaglePortApi RequirePermission(string permission)
        {
            InternalRequirePermission(permission);
            return this;
        }
    }
}
