using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Core.NetObjects.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects.Ports.Property
{
    abstract class EaglePortProperty<T> : EagleNetObjectPort, IEaglePortProperty<T>
    {
        public EaglePortProperty(EagleNetObjectInstance ctx, string name) : base(ctx, name)
        {
        }

        private T value;
        private bool webEditable;

        public T Value
        {
            get => value;
            set => SetValue(value, null);
        }

        public bool IsWebEditable
        {
            get => webEditable;
            set => webEditable = value;
        }

        public event IEaglePortProperty_Handler<T> OnChanged;

        public IEaglePortProperty<T> BindOnChanged(IEaglePortProperty_Handler<T> binding)
        {
            OnChanged += binding;
            return this;
        }

        public IEaglePortProperty<T> MakeWebEditable()
        {
            IsWebEditable = true;
            return this;
        }

        public override void OnClientConnect(EagleNetObjectClient target)
        {
            //Send client the event
            SendUpdateNotification(target);
        }

        protected override void OnClientMessage(EagleNetObjectClient client, JObject message)
        {
            //Unwrap
            string opcode;
            JObject payload;
            if (!message.TryGetString("opcode", out opcode) || !message.TryGetObject("payload", out payload))
                return;

            //Switch
            switch (opcode)
            {
                case "SET": HandleSet(client, payload); break;
            }
        }

        private void HandleSet(EagleNetObjectClient client, JObject message)
        {
            //Unwrap
            string token;
            JToken incoming;
            if (!message.TryGetString("token", out token) || !message.TryGetValue("value", out incoming))
                return;

            //Handle
            bool success;
            string error;
            try
            {
                //Make sure that this even is writable
                if (!webEditable)
                    throw new Exception("This value cannot be manipulated from the web.");

                //Validate that we have permission
                EnsureClientPermission(client);

                //Parse
                T data = WebDeserialize(incoming);

                //Apply
                SetValue(data, client);

                //Set
                success = true;
                error = null;
            }
            catch (Exception ex)
            {
                success = false;
                error = ex.Message;
            }

            //Create output
            JObject output = new JObject();
            output["token"] = token;
            output["ok"] = success;
            output["error"] = error;

            //Send
            InternalSend(client, "ACK", output);
        }

        protected void InternalSend(IEagleNetObjectTarget target, string opcode, JObject payload)
        {
            JObject output = new JObject();
            output["opcode"] = opcode;
            output["payload"] = payload;
            InternalSend(target, output);
        }

        protected override void CreateExtra(JObject extra)
        {
            //Set
            extra["writable"] = webEditable;

            //Convert permissions
            JArray arr = new JArray();
            foreach (var p in RequiredPermissions)
                arr.Add(p);
            extra["required_permissions"] = arr;
        }

        private void SendUpdateNotification(IEagleNetObjectTarget target)
        {
            //Create
            JToken data = WebSerialize(Value);

            //Wrap
            JObject wrapping = new JObject();
            wrapping["value"] = data;

            //Send
            InternalSend(target, "UPDATE", wrapping);
        }

        private void SetValue(T value, EagleNetObjectClient client)
        {
            //If this is a web account, do some validation
            if (client != null)
            {
                EnsureClientPermission(client);
            }

            //Create args
            PropertySetArgs args = new PropertySetArgs(value, client != null, this);

            //Dispatch event
            OnChanged?.Invoke(args);

            //Apply
            this.value = args.Value;

            //Send web event to all
            SendUpdateNotification(Manager);
        }

        public IEaglePortProperty<T> RequirePermission(string permission)
        {
            InternalRequirePermission(permission);
            return this;
        }

        protected abstract T WebDeserialize(JToken data);
        protected abstract JToken WebSerialize(T data);

        class PropertySetArgs : IEaglePortPropertySetArgs<T>
        {
            public PropertySetArgs(T value, bool fromWeb, EaglePortProperty<T> port)
            {
                this.value = value;
                this.fromWeb = fromWeb;
                this.port = port;
            }

            private T value;
            private bool fromWeb;
            private EaglePortProperty<T> port;

            public IEaglePortProperty<T> Port => port;
            public bool FromWeb => fromWeb;
            public T Value { get => value; set => this.value = value; }
        }
    }
}
