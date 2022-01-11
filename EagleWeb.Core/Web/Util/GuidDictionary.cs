using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.Web.Util
{
    public class GuidDictionary<T>
    {
        public GuidDictionary()
        {

        }

        private Dictionary<Guid, T> values = new Dictionary<Guid, T>();

        public Guid Put(T value)
        {
            return Put((Guid guid) => value);
        }

        public Guid Put(Func<Guid, T> getter)
        {
            return Put(getter, out T value);
        }

        public Guid Put(Func<Guid, T> getter, out T value)
        {
            Guid guid;
            lock (values)
            {
                //Generate a random unique GUID
                do
                {
                    guid = Guid.NewGuid();
                } while (values.ContainsKey(guid));

                //Create
                value = getter(guid);

                //Insert
                values.Add(guid, value);
            }
            return guid;
        }

        public bool TryGet(Guid guid, out T value)
        {
            bool result;
            lock (values)
            {
                result = values.TryGetValue(guid, out value);
            }
            return result;
        }

        public bool TryGet(string guid, out T value)
        {
            //Attempt to parse GUID
            Guid parsed;
            if (!Guid.TryParse(guid, out parsed))
            {
                value = default(T);
                return false;
            }

            //Run as normal
            return TryGet(parsed, out value);
        }

        public void ForEach(Action<Guid, T> enumerator)
        {
            lock (values)
            {
                foreach (var v in values)
                    enumerator(v.Key, v.Value);
            }
        }
    }
}
