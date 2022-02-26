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
    class EaglePortEventDispatcher : EagleNetObjectPort, IEaglePortEventDispatcher
    {
        public EaglePortEventDispatcher(EagleNetObjectInstance ctx, string name) : base(ctx, name)
        {
        }

        public override EagleNetObjectPortType PortType => EagleNetObjectPortType.PORT_DISPATCHER;

        public event IEaglePortEventDispatcher_Handler OnReceive;

        public IEaglePortEventDispatcher Bind(IEaglePortEventDispatcher_Handler handler)
        {
            OnReceive += handler;
            return this;
        }

        public override void OnClientConnect(IEagleNetObjectTarget target)
        {
            
        }

        public void Push(JObject message)
        {
            InternalSend(TargetAll, message);
        }

        protected override void CreateExtra(JObject extra)
        {
            
        }

        protected override void OnClientMessage(EagleNetObjectClient client, JObject message)
        {
            OnReceive?.Invoke(client.Account, message);
        }
    }
}
