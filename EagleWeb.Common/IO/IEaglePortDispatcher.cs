using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO
{
    /// <summary>
    /// A port that acts as an opcode dispatcher.
    /// </summary>
    public interface IEaglePortDispatcher : IEagleIdProvider
    {
        IEaglePortIO CreatePort(string opcode);
        IEaglePortDispatcher CreatePortDispatcher(string opcode);
    }
}
