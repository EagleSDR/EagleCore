using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.Plugin.Interfaces.RadioSession
{
    public interface IEagleRadioSessionModule : IEagleDestructable
    {
        /// <summary>
        /// If true, this session won't be destroyed when a user isn't looking at it. Typically, you should return false unless you're doing something with the samples (like saving them to disk).
        /// </summary>
        bool KeepAlive { get; }
    }
}
