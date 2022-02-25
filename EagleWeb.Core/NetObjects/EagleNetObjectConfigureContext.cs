using EagleWeb.Common;
using EagleWeb.Common.NetObjects;
using EagleWeb.Common.NetObjects.IO;
using EagleWeb.Core.NetObjects.Ports;
using EagleWeb.Core.NetObjects.Ports.Property;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects
{
    class EagleNetObjectConfigureContext : IEagleObjectConfigureContext
    {
        public EagleNetObjectConfigureContext(EagleNetObjectInstance ctx)
        {
            this.ctx = ctx;
        }

        private EagleNetObjectInstance ctx;

        public IEaglePortApi CreatePortApi(string name)
        {
            EaglePortApi port = new EaglePortApi(ctx, name);
            ctx.Ports.Add(port);
            return port;
        }

        public IEaglePortEventDispatcher CreateEventDispatcher(string name)
        {
            EaglePortEventDispatcher port = new EaglePortEventDispatcher(ctx, name);
            ctx.Ports.Add(port);
            return port;
        }

        public IEaglePortProperty<bool> CreatePropertyBool(string name)
        {
            EaglePortPropertyPrimitive<bool> port = new EaglePortPropertyPrimitive<bool>(ctx, name);
            ctx.Ports.Add(port);
            return port;
        }

        public IEaglePortProperty<string> CreatePropertyString(string name)
        {
            EaglePortPropertyPrimitive<string> port = new EaglePortPropertyPrimitive<string>(ctx, name);
            ctx.Ports.Add(port);
            return port;
        }

        public IEaglePortProperty<long> CreatePropertyLong(string name)
        {
            EaglePortPropertyPrimitive<long> port = new EaglePortPropertyPrimitive<long>(ctx, name);
            ctx.Ports.Add(port);
            return port;
        }

        public IEaglePortProperty<int> CreatePropertyInt(string name)
        {
            EaglePortPropertyPrimitive<int> port = new EaglePortPropertyPrimitive<int>(ctx, name);
            ctx.Ports.Add(port);
            return port;
        }

        public IEaglePortProperty<float> CreatePropertyFloat(string name)
        {
            EaglePortPropertyPrimitive<float> port = new EaglePortPropertyPrimitive<float>(ctx, name);
            ctx.Ports.Add(port);
            return port;
        }

        public IEaglePortProperty<T> CreatePropertyObject<T>(string name) where T : IEagleObject
        {
            EaglePortPropertyObject<T> port = new EaglePortPropertyObject<T>(ctx, name);
            ctx.Ports.Add(port);
            return port;
        }
    }
}
