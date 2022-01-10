using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web
{
    public interface IEagleWebServerService
    {
        Task HandleRequest(HttpContext e);
    }
}
