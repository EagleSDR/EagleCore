using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Core.NetObjects.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects
{
    interface IEagleNetObjectInternalIO
    {
        string Guid { get; }
        void OnClientConnect(IEagleTarget target);
        void OnClientMessage(IEagleClient client, EagleNetObjectOpcode opcode, JObject message);
    }
}
