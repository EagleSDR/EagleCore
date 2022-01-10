using EagleWeb.Common.Auth;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.Streams
{
    /// <summary>
    /// Describes an actual streaming client.
    /// </summary>
    public interface IEagleStreamClient
    {
        IEagleAccount Account { get; }
        object UserData { get; set; }

        void Send(byte[] data, int offset, int length, bool asText = false);
        void Send(string message);
        void Send(JObject message);

        event IEagleStreamClient_ReceiveBinaryEventArgs OnReceiveBinary;
        event IEagleStreamClient_ReceiveTextEventArgs OnReceiveText;

        event IEagleStreamClient_CloseEventArgs OnClose;
    }

    public delegate void IEagleStreamClient_ReceiveBinaryEventArgs(IEagleStreamClient client, byte[] data, int length);
    public delegate void IEagleStreamClient_ReceiveTextEventArgs(IEagleStreamClient client, string data);
    public delegate void IEagleStreamClient_CloseEventArgs(IEagleStreamClient client);
}
