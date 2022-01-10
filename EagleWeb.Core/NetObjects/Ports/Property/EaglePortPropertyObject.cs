using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Core.NetObjects.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects.Ports.Property
{
    class EaglePortPropertyObject<T> : EaglePortProperty<T> where T : EagleObject
    {
        public EaglePortPropertyObject(EagleNetObjectInstance ctx, string name) : base(ctx, name)
        {
            
        }

        public override EagleNetObjectPortType PortType => EagleNetObjectPortType.PORT_PROPERTY_OBJECT;

        protected override T WebDeserialize(JToken data)
        {
            //If it's null, then return null
            if (data.Type == JTokenType.Null)
                return null;

            //If it's a string, look up that GUID in the collection
            if (data.Type == JTokenType.String)
            {
                //Get it as a string
                string target = (string)data;

                //Search for it
                if (!Ctx.Manager.Collection.TryGetItemByGuid(target, out IEagleNetObjectInternalIO item))
                    throw new Exception($"Failed to find object {target}. The GUID is invalid.");

                //Cast to the wrapper
                if (!(item is EagleNetObjectInstance))
                    throw new Exception("The GUID specified is not an EagleObject.");

                //Extract the object
                EagleObject obj = (item as EagleNetObjectInstance).Ctx;

                //Make sure it's of the valid type
                if (!obj.GetType().IsSubclassOf(typeof(T)))
                    throw new Exception($"The object must be a subclass of \"{typeof(T).Name}\", but this object is \"{obj.GetType().Name}\".");

                return obj as T;
            }

            //Invalid format
            throw new Exception("Value is in an invalid format. Expected a string.");
        }

        protected override JToken WebSerialize(T data)
        {
            //If the value is null, return the string null
            if (data == null)
                return null;

            //Return the guid
            return data.Guid;
        }
    }
}
