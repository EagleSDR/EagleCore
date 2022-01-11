using EagleWeb.Common.IO.Sockets;
using EagleWeb.Core.Auth;
using EagleWeb.Core.Web.Util;
using EagleWeb.Core.Web.WS;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web.Sockets
{
    class EagleSocketManager : EagleWsConnectionService2
    {
        public EagleSocketManager(EagleContext ctx) : base(ctx)
        {
        }

        private Dictionary<string, EagleSocketServer> registeredNames = new Dictionary<string, EagleSocketServer>();
        private GuidDictionary<EagleSocketServer> registered = new GuidDictionary<EagleSocketServer>();

        public IEagleSocketServer RegisterServer(string friendlyName, IEagleSocketHandler handler)
        {
            //Put in the registered collection
            registered.Put((Guid guid) => new EagleSocketServer(Ctx, guid, friendlyName, handler), out EagleSocketServer server);

            //Add to the name lookup
            lock (registeredNames)
            {
                //Check if it already exists
                if (registeredNames.ContainsKey(friendlyName))
                    Ctx.Log(Common.EagleLogLevel.WARN, "EagleSocketManager", $"Multiple sockets registered with friendly name \"{friendlyName}\"! This may cause conflicts...");
                else
                    registeredNames.Add(friendlyName, server);
            }

            return server;
        }

        /// <summary>
        /// Searches for the specified socket using either the ID or the friendly name. Returns if it was successful or not.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool Search(HttpContext e, out EagleSocketServer result)
        {
            //Check if we're searching by ID
            if (e.Request.Query.TryGetString("sock_id", out string id))
            {
                return registered.TryGet(id, out result);
            }

            //Check if we're searching by friendly name
            if (e.Request.Query.TryGetString("sock_name", out string name))
            {
                lock (registeredNames)
                    return registeredNames.TryGetValue(name, out result);
            }

            //Nothing specified.
            result = null;
            return false;
        } 

        protected override bool CreateConnection(HttpContext e, EagleAccount account, out EagleBaseConnection connection)
        {
            //Look for any specified search methods
            EagleSocketServer server;
            if (!Search(e, out server))
            {
                e.Response.StatusCode = 404;
                connection = null;
                return false;
            }

            //Create client, it'll take care of the rest
            connection = new EagleSocketClient(server, account);
            return true;
        }
    }
}
