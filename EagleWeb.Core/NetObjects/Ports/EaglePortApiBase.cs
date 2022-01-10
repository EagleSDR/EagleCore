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
    abstract class EaglePortApiBase : EagleNetObjectPort
    {
        public EaglePortApiBase(EagleNetObjectInstance ctx, string name) : base(ctx, name)
        {
        }

        protected override void OnClientMessage(IEagleClient client, JObject message)
        {
            //Unwrap
            string token;
            JObject payload;
            if (!message.TryGetString("token", out token) || !message.TryGetObject("payload", out payload))
                return;

            //Handle
            bool success;
            string error;
            JObject result;
            try
            {
                //Validate that we have permission
                EnsureClientPermission(client);

                //Run
                result = OnClientApiCommand(client, payload);

                //Set
                success = true;
                error = null;
            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message;
                result = null;
            }

            //Create output
            JObject output = new JObject();
            output["token"] = token;
            output["ok"] = success;
            output["error"] = error;
            output["result"] = result;

            //Send
            InternalSend(client, output);
        }

        protected abstract JObject OnClientApiCommand(IEagleClient client, JObject message);
    }
}
