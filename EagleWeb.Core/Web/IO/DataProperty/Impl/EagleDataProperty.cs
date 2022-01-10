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
    public class EagleDataProperty<T> : IEagleDataProperty<T>, IEagleDataPropertyImpl
    {
        public EagleDataProperty(string id, IEaglePortDispatcher dispatcher, T value)
        {
            this.id = id;
            this.value = value;
            portSet = dispatcher.CreatePort("SET");
            portSet.OnClientConnect += PortSet_OnClientConnect;
        }

        protected readonly string id;
        protected T value;
        protected IEaglePortIO portSet;

        protected readonly List<string> requiredPermissions = new List<string>();

        public string Id => id;
        public T Value
        {
            get => value;
            set
            {
                this.value = value;
                DispatchUpdateAll();
            }
        }

        public Type Type => typeof(T);
        public virtual bool WebWritable => false;
        public IReadOnlyList<string> WebRequiredPermissions => requiredPermissions;

        private void DispatchUpdateAll()
        {
            DispatchUpdate(portSet.TargetAll);
        }

        private void DispatchUpdate(IEagleTarget target)
        {
            //Create event
            JObject payload = new JObject();
            payload["value"] = value == null ? null : JToken.FromObject(value);

            //Send
            portSet.Send(payload, target);
        }

        private void PortSet_OnClientConnect(IEagleClient client)
        {
            DispatchUpdate(client);
        }
    }
}
