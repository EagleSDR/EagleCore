using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.Sockets
{
    /// <summary>
    /// An interface implemented by the user to handle socket events.
    /// </summary>
    public interface IEagleSocketHandler
    {
        /// <summary>
        /// Called when a server is created for this socket.
        /// </summary>
        /// <param name="context"></param>
        void OnCreate(IEagleSocketServer context);

        /// <summary>
        /// Called when a client connects. Set events on the client here.
        /// </summary>
        /// <param name="client"></param>
        void OnClientConnect(IEagleSocketClient client);

        /// <summary>
        /// Called when a client disconnects.
        /// </summary>
        void OnClientDisconnect(IEagleSocketClient client);
    }
}
