using EagleWeb.Core.NetObjects.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects
{
    interface IEagleNetObjectTarget
    {
        void SendMessage(EagleNetObjectOpcode opcode, string guid, JObject payload);
    }
}
