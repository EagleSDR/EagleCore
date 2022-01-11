using EagleWeb.Common.Auth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.NetObjects.IO
{
    public delegate JObject IEaglePortApi_Handler(IEagleAccount account, JObject message);

    public interface IEaglePortApi : IEagleObjectPort
    {
        IEaglePortApi RequirePermission(string permission);
        IEaglePortApi Bind(IEaglePortApi_Handler handler);
    }
}
