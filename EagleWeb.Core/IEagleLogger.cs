using EagleWeb.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core
{
    public interface IEagleLogger
    {
        void Log(EagleLogLevel level, string topic, string message);
    }
}
