using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.NetObjects
{
    /// <summary>
    /// Context for talking to the internal item for this.
    /// </summary>
    public interface IEagleObjectInternalContext
    {
        /// <summary>
        /// The GUID of this object.
        /// </summary>
        public string Guid { get; }

        /// <summary>
        /// Begins creation of an object.
        /// </summary>
        /// <returns></returns>
        IEagleObjectConfigureContext BeginCreate();

        /// <summary>
        /// Called when we are done with the configure context.
        /// </summary>
        void EndCreate(IEagleObjectConfigureContext ctx);

        /// <summary>
        /// Logs to the console output.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="level"></param>
        /// <param name="topic"></param>
        /// <param name="message"></param>
        void Log(EagleLogLevel level, string topic, string message);

        /// <summary>
        /// Notifies clients that we are being destroyed.
        /// </summary>
        void Destroy();
    }
}
