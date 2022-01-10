using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects.Enums
{
    enum EagleNetObjectOpcode
    {
        OBJECT_CREATE = 1,
        OBJECT_DESTROY = 2,
        IO_MESSAGE = 3,
        SET_CONTROL_OBJECT = 4
    }
}
