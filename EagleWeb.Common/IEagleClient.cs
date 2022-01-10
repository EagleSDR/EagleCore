using EagleWeb.Common.Auth;
using EagleWeb.Common.IO;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common
{
    /// <summary>
    /// Defines a web client.
    /// </summary>
    public interface IEagleClient : IEagleTarget
    {
        IEagleAccount Account { get; }
    }
}
