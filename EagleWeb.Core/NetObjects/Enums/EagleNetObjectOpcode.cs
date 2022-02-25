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
        SET_CONTROL_OBJECT = 4,
        OBJECT_READY = 5,
        OBJECT_PING = 6,
        OBJECT_REQUEST_DESTRUCTION = 7
    }
}
