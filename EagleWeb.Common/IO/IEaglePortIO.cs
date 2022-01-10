using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO
{
    /// <summary>
    /// A port that can tx/rx arbitrary data to an implementation.
    /// </summary>
    public interface IEaglePortIO : IEagleIdProvider
    {
        /// <summary>
        /// Sends an outgoing message to the target(s).
        /// </summary>
        /// <param name="payload">The payload to send.</param>
        /// <param name="target">The target(s) to deliver the message to.</param>
        void Send(JObject payload, IEagleTarget target);

        /// <summary>
        /// Event called when an incoming message arrives.
        /// </summary>
        event EaglePortIO_OnReceiveEventArgs OnReceive;

        /// <summary>
        /// Event called when a new client connects.
        /// </summary>
        event EaglePortIO_OnConnectEventArgs OnClientConnect;

        /// <summary>
        /// Event called when a new client disconnects.
        /// </summary>
        event EaglePortIO_OnConnectEventArgs OnClientDisconnect;

        /// <summary>
        /// A target defining all clients.
        /// </summary>
        IEagleTarget TargetAll { get; }
    }

    public delegate void EaglePortIO_OnReceiveEventArgs(IEagleClient client, JObject payload);
    public delegate void EaglePortIO_OnConnectEventArgs(IEagleClient client);
}
