using EagleWeb.Common.IO.Sockets;
using EagleWeb.Common.NetObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Plugin
{
    public interface IEagleObjectPluginContext : IEagleObjectManagerLink
    {
        IEagleContext Context { get; }

        /// <summary>
        /// Registers a socket, which allows plugins to send arbitrary binary data over the network.
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        IEagleSocketServer RegisterSocketServer(string friendlyName, IEagleSocketHandler handler);
    }
}
