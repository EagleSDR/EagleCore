using EagleWeb.Common.IO.Sockets;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.Plugin.Interfaces.Radio;
using EagleWeb.Common.Plugin.Interfaces.RadioSession;
using EagleWeb.Common.Radio;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Plugin
{
    public delegate void IEaglePluginContext_OnInitEventArgs();

    public interface IEaglePluginContext
    {
        /// <summary>
        /// Gets the global context.
        /// </summary>
        IEagleContext Context { get; }

        /// <summary>
        /// Event fired when all plugins are finished being created. Must be set in the constructor to ensure it isn't set late.
        /// </summary>
        event IEaglePluginContext_OnInitEventArgs OnInit;

        /// <summary>
        /// Creates a new web object that is replicated across the web.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="creator">A callback that you should construct the object within.</param>
        /// <returns>The object you created and returned within the callback.</returns>
        T CreateObject<T>(Func<IEagleObjectContext, T> creator) where T : IEagleObject;

        /// <summary>
        /// Creates a static EagleObject that can be accessed directly from the web using the key you provide.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The identifier you use to access this object from the web.</param>
        /// <param name="creator">A callback that you should construct the object within.</param>
        /// <returns>The object you created and returned within the callback.</returns>
        T CreateStaticObject<T>(string key, Func<IEagleObjectContext, T> creator) where T : IEagleObject;

        /// <summary>
        /// Registers a socket, which allows plugins to send arbitrary binary data over the network.
        /// </summary>
        /// <param name="friendlyName"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        IEagleSocketServer RegisterSocketServer(string friendlyName, IEagleSocketHandler handler);

        /// <summary>
        /// Registers a module that'll be created with the radio. Do this only during plugin initialization.
        /// </summary>
        /// <param name="id">A plugin-unique identifier that can be used to get the same item on the web.</param>
        /// <param name="module"></param>
        void RegisterModuleRadio(string id, Func<IEagleRadio, IEagleRadioModule> module);

        /// <summary>
        /// Registers a module that'll be created with the radio session. Do this only during plugin initialization.
        /// </summary>
        /// <param name="id">A plugin-unique identifier that can be used to get the same item on the web.</param>
        /// <param name="module"></param>
        void RegisterModuleRadioSession(string id, Func<IEagleRadioSession, IEagleRadioSessionModule> module);

        /// <summary>
        /// Prints a log message.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        void Log(EagleLogLevel level, string topic, string message);
    }
}
