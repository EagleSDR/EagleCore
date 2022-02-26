using EagleWeb.Common.NetObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common
{
    public interface IEagleObject : IEagleDestructable
    {
        string Guid { get; }
        T CreateChildObject<T>(Func<IEagleObjectContext, T> creator) where T : IEagleObject;
        void Log(EagleLogLevel level, string topic, string message);
    }
}
