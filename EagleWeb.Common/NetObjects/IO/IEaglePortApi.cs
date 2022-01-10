using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.NetObjects.IO
{
    public delegate JObject IEaglePortApi_Handler(IEagleClient client, JObject message);

    public interface IEaglePortApi : IEagleObjectPort
    {
        IEaglePortApi RequirePermission(string permission);
        IEaglePortApi Bind(IEaglePortApi_Handler handler);
    }
}
