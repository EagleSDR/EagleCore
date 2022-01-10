using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects
{
    class EagleGuidObjectCollection
    {
        public EagleGuidObjectCollection()
        {
        }

        private readonly ConcurrentDictionary<Guid, ReservationWrapper> map = new ConcurrentDictionary<Guid, ReservationWrapper>();
        private readonly List<ReservationWrapper> items = new List<ReservationWrapper>();

        public string ReserveGuid()
        {
            //Make wrapper
            ReservationWrapper wrapper = new ReservationWrapper();

            //Add to the map
            Guid guid;
            do
            {
                guid = Guid.NewGuid();
            } while (!map.TryAdd(guid, wrapper));

            //Add to the list
            lock (items)
                items.Add(wrapper);

            return guid.ToString();
        }

        public void ActivateGuid(IEagleNetObjectInternalIO item)
        {
            //Parse GUID
            if (!Guid.TryParse(item.Guid, out Guid result))
                return;

            //Activate
            lock (map)
                map[result].item = item;
        }

        public void DeactivateGuid(IEagleNetObjectInternalIO item)
        {
            //Parse GUID
            if (!Guid.TryParse(item.Guid, out Guid result))
                return;

            //Activate
            lock (map)
                map[result].item = null;
        }

        public bool TryGetItemByGuid(string guid, out IEagleNetObjectInternalIO item)
        {
            //Parse GUID
            if (!Guid.TryParse(guid, out Guid result))
            {
                item = null;
                return false;
            }

            //Run
            if (map.TryGetValue(result, out ReservationWrapper value) && value.item != null)
            {
                item = value.item;
                return true;
            } else
            {
                item = null;
                return false;
            }
        }

        public void Enumerate(Action<IEagleNetObjectInternalIO> cb)
        {
            lock (items)
            {
                foreach (var v in items)
                {
                    if (v.item != null)
                        cb(v.item);
                }
            }
        }

        class ReservationWrapper
        {
            public IEagleNetObjectInternalIO item = null;
        }
    }
}
