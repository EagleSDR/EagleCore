using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Common.Plugin.Interfaces.Radio;
using EagleWeb.Common.Radio.RDS;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Radio
{
    public delegate void IEagleRadio_SessionEventArgs(IEagleRadio radio, IEagleRadioSession session);
    public interface IEagleRadio : IEagleObject
    {
        /// <summary>
        /// Event fired when a new session is created.
        /// </summary>
        event IEagleRadio_SessionEventArgs OnSessionCreated;

        /// <summary>
        /// Event fired when a new session is destroyed.
        /// </summary>
        event IEagleRadio_SessionEventArgs OnSessionRemoved;
    }
}
