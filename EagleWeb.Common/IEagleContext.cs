using EagleWeb.Common.IO.FileSystem;
using EagleWeb.Common.IO.Sockets;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.Radio;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common
{
    /// <summary>
    /// Context to the Eagle server.
    /// </summary>
    public interface IEagleContext
    {
        int BufferSize { get; }
        IEagleRadio Radio { get; }

        T CreateObject<T>(Func<IEagleObjectContext, T> creator) where T : IEagleObject;
        WebFsFileStream ResolveFileToken(string token);
        bool TryResolveWebGuid<T>(string guid, out T obj) where T : IEagleObject;
    }
}
