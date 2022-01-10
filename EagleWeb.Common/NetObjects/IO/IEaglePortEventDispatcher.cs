using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.NetObjects.IO
{
    public delegate void IEaglePortEventDispatcher_Handler(IEagleClient client, JObject message);

    public interface IEaglePortEventDispatcher : IEagleObjectPort
    {
        /// <summary>
        /// Fired when a client sends an event to us.
        /// </summary>
        event IEaglePortEventDispatcher_Handler OnReceive;

        /// <summary>
        /// Binds to the OnReceive event in a more builder-friendly manner.
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        IEaglePortEventDispatcher Bind(IEaglePortEventDispatcher_Handler handler);

        /// <summary>
        /// Pushes an event out to clients.
        /// </summary>
        /// <param name="message"></param>
        void Push(JObject message);
    }
}
