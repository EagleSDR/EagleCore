using EagleWeb.Common;
using EagleWeb.Common.Auth;
using EagleWeb.Common.IO;
using EagleWeb.Common.NetObjects;
using EagleWeb.Core.Auth;
using EagleWeb.Core.NetObjects.Enums;
using EagleWeb.Core.NetObjects.Misc;
using EagleWeb.Core.Web.WS;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects
{
    class EagleNetObjectManager : EagleWsConnectionService2
    {
        public EagleNetObjectManager(EagleContext ctx) : base(ctx)
        {
        }

        public EagleGuidObjectCollection Collection => collection;
        public EagleObject Control => control;

        private EagleGuidObjectCollection collection = new EagleGuidObjectCollection();
        private List<EagleNetObjectClient> clients = new List<EagleNetObjectClient>();
        private EagleObject control;

        public T CreateObject<T>(Func<IEagleObjectContext, T> creator) where T : IEagleObject
        {
            return CreateObject(creator, new FilteredTarget(this));
        }

        public T CreateObject<T>(Func<IEagleObjectContext, T> creator, IEagleFilteredTarget target) where T : IEagleObject
        {
            //Reserve a unique GUID
            string guid = collection.ReserveGuid();

            //Create an instance using that ID
            EagleNetObjectInstance instance = new EagleNetObjectInstance(guid, this, target);

            //Call the creator code to obtain the user methods
            T user = creator(instance);

            //Activate registration
            collection.ActivateGuid(instance);

            //Apply
            instance.Finalize(user);

            return user;
        }

        public void SetControlObject(EagleObject control)
        {
            this.control = control;
        }

        public void AddClient(EagleNetObjectClient client)
        {
            lock (clients)
                clients.Add(client);
        }

        public void RemoveClient(EagleNetObjectClient client)
        {
            lock (clients)
                clients.Remove(client);
        }

        protected override bool CreateConnection(HttpContext e, EagleAccount account, out EagleBaseConnection connection)
        {
            //Create wrapper
            EagleNetObjectClient client = new EagleNetObjectClient(this, account);

            connection = client;
            return true;
        }

        public static byte[] EncodeMessage(EagleNetObjectOpcode opcode, string guid, JObject payload)
        {
            //Wrap
            JObject msg = new JObject();
            msg["o"] = (int)opcode;
            msg["g"] = guid;
            msg["p"] = payload;

            //Serialize
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(msg));
        }

        public bool TryResolveWebGuid<T>(string guid, out T obj) where T : IEagleObject
        {
            //Search for an object with this GUID
            if (collection.TryGetItemByGuid(guid, out IEagleNetObjectInternalIO item) && item is EagleNetObjectInstance objContainer && objContainer.User is T o)
            {
                obj = o;
                return true;
            } else
            {
                obj = default(T);
                return false;
            }
        }

        class FilteredTarget : IEagleFilteredTarget
        {
            public FilteredTarget(EagleNetObjectManager manager)
            {
                this.manager = manager;
            }

            private readonly EagleNetObjectManager manager;
            private readonly List<string> accounts = new List<string>();

            public void AddAccountFilter(IEagleAccount account)
            {
                accounts.Add(account.Username);
            }

            public void SendMessage(EagleNetObjectOpcode opcode, string guid, JObject payload)
            {
                //Search for clients
                List<EagleNetObjectClient> clients = new List<EagleNetObjectClient>();
                lock (manager.clients)
                {
                    if (accounts.Count == 0)
                    {
                        //Add all
                        clients.AddRange(manager.clients);
                    } else
                    {
                        //Apply filter
                        foreach (var c in manager.clients)
                        {
                            if (accounts.Contains(c.Account.Username))
                                clients.Add(c);
                        }
                    }
                }

                //Encode
                byte[] data = EncodeMessage(opcode, guid, payload);

                //Dispatch to all
                foreach (var c in clients)
                    c.Send(data, 0, data.Length, true);
            }

            public IEagleFilteredTarget Clone()
            {
                //Make
                FilteredTarget target = new FilteredTarget(manager);

                //Copy over accounts
                target.accounts.AddRange(accounts);

                return target;
            }
        }
    }
}
