using EagleWeb.Common;
using EagleWeb.Common.IO;
using EagleWeb.Common.NetObjects;
using EagleWeb.Core.NetObjects.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Core.NetObjects
{
    /// <summary>
    /// IO for a specific opcode inside of a EagleNetObject.
    /// </summary>
    abstract class EagleNetObjectPort : IEagleNetObjectInternalIO, IEagleObjectPort
    {
        public EagleNetObjectPort(EagleNetObjectInstance ctx, string name)
        {
            this.ctx = ctx;
            this.name = name;
            guid = Manager.Collection.ReserveGuid();
            Manager.Collection.ActivateGuid(this);
            ctx.OnDestroyed += HostDestroyed;
        }

        private readonly EagleNetObjectInstance ctx;
        private readonly string name;
        private readonly string guid;
        private readonly List<string> requiredPermissions = new List<string>();

        public EagleNetObjectInstance Ctx => ctx;
        public IEagleNetObjectTarget TargetAll => ctx.TargetAll;
        public EagleNetObjectManager Manager => ctx.Manager;
        public string Guid => guid;
        public string Name => name;
        public List<string> RequiredPermissions => requiredPermissions;

        public abstract EagleNetObjectPortType PortType { get; }
        protected abstract void CreateExtra(JObject extra);

        public abstract void OnClientConnect(IEagleNetObjectTarget client);
        protected abstract void OnClientMessage(EagleNetObjectClient client, JObject message);

        public void OnClientMessage(EagleNetObjectClient client, EagleNetObjectOpcode opcode, JObject message)
        {
            if (opcode == EagleNetObjectOpcode.IO_MESSAGE)
                OnClientMessage(client, message);
        }

        protected void InternalSend(IEagleNetObjectTarget target, JObject payload)
        {
            target.SendMessage(EagleNetObjectOpcode.IO_MESSAGE, guid, payload);
        }

        protected bool CheckClientPermission(EagleNetObjectClient client)
        {
            //Make sure the client has all permissions
            foreach (var p in requiredPermissions)
            {
                if (!client.Account.HasPermission(p))
                    return false;
            }

            return true;
        }

        protected void EnsureClientPermission(EagleNetObjectClient client)
        {
            if (!CheckClientPermission(client))
                throw new Exception("Sorry, you do not have permission to perform this operation.");
        }

        public JObject CreateInfo()
        {
            //Create extra
            JObject extra = new JObject();
            CreateExtra(extra);

            //Make
            JObject msg = new JObject();
            msg["guid"] = guid;
            msg["name"] = name;
            msg["type"] = PortType.ToString();
            msg["info"] = extra;
            return msg;
        }

        protected void InternalRequirePermission(string permission)
        {
            requiredPermissions.Add(permission);
        }

        private void HostDestroyed()
        {
            //Deactivate the GUID to stop sending events
            Manager.Collection.DeactivateGuid(this);
        }
    }
}
