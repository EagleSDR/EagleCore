using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.Sockets
{
    /// <summary>
    /// The server obtained when registering.
    /// </summary>
    public interface IEagleSocketServer
    {
        /// <summary>
        /// The global EagleContext.
        /// </summary>
        IEagleContext Context { get; }

        /// <summary>
        /// The GUID of the server.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Queues the data for transmission to all connected clients.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="asText"></param>
        void SendAll(byte[] data, int offset, int count, bool asText);
    }
}
