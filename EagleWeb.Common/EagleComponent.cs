using EagleWeb.Common.IO;
using EagleWeb.Common.IO.DataProperty;
using EagleWeb.Common.IO.FileSystem;
using EagleWeb.Common.IO.Streams;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common
{
    /// <summary>
    /// Represents an object with an ID.
    /// </summary>
    public class EagleComponent
    {
        public EagleComponent(EagleComponent parent, string id)
        {
            this.parent = parent;
            this.id = id;
        }

        private readonly EagleComponent parent;
        private readonly string id;

        public virtual string Id => parent.Id + "." + id;
        protected virtual IEagleContext Context => parent.Context;

        protected virtual void Log(EagleLogLevel level, string topic, string message)
        {
            parent.Log(level, topic, message);
        }

        protected void Log(EagleLogLevel level, string message)
        {
            Log(level, GetType().Name, message);
        }

        private string CreateSubId(string subId)
        {
            return id + "." + subId;
        }

        protected virtual IEagleDataProperty<T> CreatePropertyReadOnly<T>(string id, T defaultValue)
        {
            return parent.CreatePropertyReadOnly(CreateSubId(id), defaultValue);
        }

        protected virtual IEagleDataPropertyWritable<T> CreatePropertyWritable<T>(string id, T defaultValue)
        {
            return parent.CreatePropertyWritable(CreateSubId(id), defaultValue);
        }

        protected virtual IEagleStream CreateStreamServer(string id, IEagleStreamServer server)
        {
            return parent.CreateStreamServer(CreateSubId(id), server);
        }

        protected virtual IEaglePortIO CreatePort(string subId)
        {
            return parent.CreatePort(CreateSubId(subId));
        }

        protected virtual WebFsFileStream ResolveFileToken(string token)
        {
            return parent.ResolveFileToken(token);
        }
    }
}
