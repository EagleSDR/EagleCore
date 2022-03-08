using EagleWeb.Common.Auth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.Sockets
{
    public delegate void IEagleSocketClient_OnReceiveJsonArgs(IEagleSocketClient client, JObject data);
    public delegate void IEagleSocketClient_OnReceiveArgs(IEagleSocketClient client, byte[] data, int count, bool asText);
    public delegate void IEagleSocketClient_OnCloseArgs(IEagleSocketClient client);

    /// <summary>
    /// Represents a connected client.
    /// </summary>
    public interface IEagleSocketClient
    {
        /// <summary>
        /// The global EagleContext.
        /// </summary>
        IEagleContext Context { get; }

        /// <summary>
        /// The logged-in account.
        /// </summary>
        IEagleAccount Account { get; }

        /// <summary>
        /// Custom user attribute. Can be used to store any kind of context wanted.
        /// </summary>
        object Custom { get; set; }

        /// <summary>
        /// Queues the data for transmission. Does not wait for the data to be delivered.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <param name="asText"></param>
        void Send(byte[] data, int offset, int count, bool asText);

        /// <summary>
        /// Immediately sends the content out to the client and waits for the transaction to complete. Uses less memory than Send, but hangs until the data is fully sent. Use sparingly and avoid on radio threads. Returns if it was successfully sent or not.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="count"></param>
        /// <param name="asText"></param>
        /// <returns></returns>
        bool SendNow(byte[] data, int index, int count, bool asText);

        /// <summary>
        /// Fired when data is recieved from the client.
        /// </summary>
        event IEagleSocketClient_OnReceiveArgs OnReceive;

        /// <summary>
        /// Fired when data is recieved from the client, decoded as JSON
        /// </summary>
        event IEagleSocketClient_OnReceiveJsonArgs OnReceiveJson;

        /// <summary>
        /// Fired when this socket is closed.
        /// </summary>
        event IEagleSocketClient_OnCloseArgs OnClose;
    }
}
