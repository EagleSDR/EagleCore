using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Common.NetObjects;
using EagleWeb.Core.Auth;
using EagleWeb.Core.NetObjects.Enums;
using EagleWeb.Core.Web.WS;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects
{
    class EagleNetObjectManager : EagleWsConnectionService2, IEagleObjectManager, IEagleNetObjectTarget
    {
        public EagleNetObjectManager(EagleContext ctx) : base(ctx)
        {
        }

        public EagleGuidObjectCollection Collection => collection;
        public EagleObject Control => control;

        private EagleGuidObjectCollection collection = new EagleGuidObjectCollection();
        private List<EagleNetObjectClient> clients = new List<EagleNetObjectClient>();
        private EagleObject control;

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

        public IEagleObjectInternalContext CreateObject(EagleObject ctx, JObject constructorInfo)
        {
            //Generate a unique GUID
            string guid = collection.ReserveGuid();

            //If the constructor info is null, create a filler value
            if (constructorInfo == null)
                constructorInfo = new JObject();

            //Create instance
            return new EagleNetObjectInstance(guid, ctx, constructorInfo);
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

        public void SendMessage(EagleNetObjectOpcode opcode, string guid, JObject payload)
        {
            //Encode
            byte[] data = EncodeMessage(opcode, guid, payload);

            //Send to all
            lock(clients)
            {
                foreach (var c in clients)
                    c.Send(data, 0, data.Length, true);
            }
        }

        public bool TryResolveWebGuid<T>(string guid, out T obj) where T : IEagleObject
        {
            //Search for an object with this GUID
            if (collection.TryGetItemByGuid(guid, out IEagleNetObjectInternalIO item) && item is EagleNetObjectInstance objContainer && objContainer.Ctx is T o)
            {
                obj = o;
                return true;
            } else
            {
                obj = default(T);
                return false;
            }
        }
    }
}
