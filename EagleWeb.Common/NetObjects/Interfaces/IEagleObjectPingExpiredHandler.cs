using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.NetObjects.Interfaces
{
    public interface IEagleObjectPingExpiredHandler
    {
        /// <summary>
        /// Called when the web object stops getting pings, effectively signalling that the owner no longer uses it.
        /// Return false to continue to destruction, otherwise return true to abort destruction and wait another timeout period. If the timeout period elapses again, this method will be called again.
        /// </summary>
        /// <returns></returns>
        bool WebPingTimeout();
    }
}
