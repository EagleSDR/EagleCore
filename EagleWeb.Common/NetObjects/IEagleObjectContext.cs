using EagleWeb.Common.NetObjects.Interfaces;
using EagleWeb.Common.NetObjects.IO;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.NetObjects
{
    public delegate void IEagleObjectConfigureContext_OnDestroyedEventArgs();

    /// <summary>
    /// This interface allows you to set up an object. Here is where you should be creating properties.
    /// </summary>
    public interface IEagleObjectContext
    {
        /// <summary>
        /// The object GUID
        /// </summary>
        string Guid { get; }

        /// <summary>
        /// The core context
        /// </summary>
        IEagleContext Context { get; }

        /// <summary>
        /// Event raised when this object is destroyed.
        /// </summary>
        event IEagleObjectConfigureContext_OnDestroyedEventArgs OnDestroyed;

        /// <summary>
        /// Creates a sub-object with the same permissions as this one.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="creator"></param>
        /// <returns></returns>
        T CreateChildObject<T>(Func<IEagleObjectContext, T> creator) where T : IEagleObject;

        /// <summary>
        /// When called, makes it so the client must continously ping the object to keep it alive. This'll allow the object to be disposed if nobody is using it. Pings are handled for you.
        /// </summary>
        /// <param name="handler">An optional handler that'll allow you to override the operation.</param>
        void RequireKeepAlivePings(IEagleObjectPingExpiredHandler handler = null);

        /// <summary>
        /// When calls, allows web clients to remotely delete and dispose this object at will.
        /// </summary>
        void AllowWebDeletion();

        /// <summary>
        /// Adds custom user-defined data that can be accessed instantly on the web using the same key.
        /// </summary>
        /// <param name="key">The unique key to store the data as.</param>
        /// <param name="value">The data to be available on the web.</param>
        void AddExtra(string key, JObject value);

        /// <summary>
        /// Creates a new API port. An API port allows web clients to make requests to the server.
        /// </summary>
        /// <param name="name">Friendly name.</param>
        /// <returns></returns>
        IEaglePortApi CreatePortApi(string name);

        /// <summary>
        /// Creates a new event dispatcher, a way to send/receive events to/from clients.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEaglePortEventDispatcher CreateEventDispatcher(string name);

        /// <summary>
        /// Creates a new property, a value replicated on web clients.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEaglePortProperty<bool> CreatePropertyBool(string name);

        /// <summary>
        /// Creates a new property, a value replicated on web clients.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEaglePortProperty<string> CreatePropertyString(string name);

        /// <summary>
        /// Creates a new property, a value replicated on web clients.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEaglePortProperty<long> CreatePropertyLong(string name);

        /// <summary>
        /// Creates a new property, a value replicated on web clients.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEaglePortProperty<int> CreatePropertyInt(string name);

        /// <summary>
        /// Creates a new property, a value replicated on web clients.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEaglePortProperty<float> CreatePropertyFloat(string name);

        /// <summary>
        /// Creates a new property, a value replicated on web clients. Used to pick EagleObjects.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IEaglePortProperty<T> CreatePropertyObject<T>(string name) where T : IEagleObject;

        /// <summary>
        /// Logs on the behalf of this object.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        void Log(EagleLogLevel level, string topic, string message);

        /// <summary>
        /// Triggers destruction of the object on the web side.
        /// </summary>
        void Destroy();
    }
}
