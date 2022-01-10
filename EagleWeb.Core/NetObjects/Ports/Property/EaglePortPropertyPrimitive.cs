using EagleWeb.Core.NetObjects.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects.Ports.Property
{
    class EaglePortPropertyPrimitive<T> : EaglePortProperty<T>
    {
        public EaglePortPropertyPrimitive(EagleNetObjectInstance ctx, string name) : base(ctx, name)
        {
        }

        public override EagleNetObjectPortType PortType => EagleNetObjectPortType.PORT_PROPERTY_PRIMITIVE;

        protected override T WebDeserialize(JToken data)
        {
            return data.ToObject<T>();
        }

        protected override JToken WebSerialize(T data)
        {
            return JToken.FromObject(data);
        }
    }
}
