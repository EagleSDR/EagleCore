using EagleWeb.Common;
using EagleWeb.Common.Auth;
using EagleWeb.Common.IO;
using EagleWeb.Common.IO.DataProperty;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web.IO.DataProperty.Impl
{
    public class EagleDataPropertyWritable<T> : EagleDataProperty<T>, IEagleDataPropertyWritable<T>
    {
        public EagleDataPropertyWritable(string id, IEaglePortDispatcher dispatcher, T value) : base(id, dispatcher, value)
        {
            portAck = dispatcher.CreatePort("ACK");
            portSet.OnReceive += PortSet_OnReceive;
        }

        private readonly IEaglePortIO portAck;

        public event EagleDataPropertyWritable_OnWebSetEventArgs<T> OnWebSet;

        public override bool WebWritable => true;

        public IEagleDataPropertyWritable<T> RequirePermission(string permission)
        {
            requiredPermissions.Add(permission);
            return this;
        }

        private void PortSet_OnReceive(IEagleClient client, JObject payload)
        {
            //Prepare response
            JObject ack = new JObject();
            ack["ok"] = true;
            ack["message"] = "";

            //Attempt to apply
            try
            {
                //Make sure the client is qualified for this...
                foreach (var p in requiredPermissions)
                    client.Account.EnsureHasPermission(p);

                //Attempt to get the value
                if (!payload.TryGetValue("value", out JToken valueToken))
                    throw new EagleDataPropertySetException(EagleDataPropertySetStatus.MALFORMED_INPUT, "Missing \"value\" property.");

                //Attempt to parse the incoming value...
                T value;
                try
                {
                    value = valueToken.ToObject<T>();
                } catch
                {
                    throw new EagleDataPropertySetException(EagleDataPropertySetStatus.MALFORMED_INPUT, "Value property is invalid and cannot be used for this type.");
                }

                //Apply
                OnWebSet?.Invoke(this, value);
                Value = value;
            }
            catch (Exception ex)
            {
                ack["ok"] = false;
                ack["message"] = ex.Message;
            }

            //Send
            portAck.Send(ack, client);
        }
    }
}
