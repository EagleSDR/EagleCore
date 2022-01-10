using EagleWeb.Common.IO;
using EagleWeb.Common.IO.DataProperty;
using EagleWeb.Common.IO.FileSystem;
using EagleWeb.Common.IO.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common
{
    /// <summary>
    /// Defines any external plugin component's context
    /// </summary>
    public interface IEagleComponent : IEagleIdProvider
    {
        IEagleContext Context { get; }

        /// <summary>
        /// Logs a message to the console.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        void Log(EagleLogLevel level, string topic, string message);

        /// <summary>
        /// Creates a property that is replicated on the web, but cannot be written to from the web.
        /// </summary>
        /// <typeparam name="T">The type to use.</typeparam>
        /// <param name="id">The component-unique opcode to use.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns></returns>
        public IEagleDataProperty<T> CreatePropertyReadOnly<T>(string id, T defaultValue);

        /// <summary>
        /// Creates a property that is replicated on and writable by the web.
        /// </summary>
        /// <typeparam name="T">The type to use.</typeparam>
        /// <param name="id">The component-unique opcode to use.</param>
        /// <param name="defaultValue">The default value to use.</param>
        /// <returns></returns>
        public IEagleDataPropertyWritable<T> CreatePropertyWritable<T>(string id, T defaultValue);

        /// <summary>
        /// Creates a new streaming server using parameters.
        /// </summary>
        /// <param name="id">The component-unique opcode to use.</param>
        /// <param name="server">The user-code to be run when connections are made.</param>
        /// <returns></returns>
        public IEagleStream CreateStreamServer(string id, IEagleStreamServer server);

        /// <summary>
        /// Creates a new port with this ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEaglePortIO CreatePort(string id);

        /// <summary>
        /// Resolves a file token to a stream, otherwise throws an exception.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public WebFsFileStream ResolveFileToken(string token);
    }
}
