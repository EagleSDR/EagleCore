using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace EagleWeb.Launcher
{
    class EagleDeveloperServer
    {
        public EagleDeveloperServer(EagleApplication app, int port = 60344)
        {
            //Set
            this.app = app;
            this.port = port;

            //Create server
            server = new Socket(SocketType.Stream, ProtocolType.Tcp);
            server.Bind(new IPEndPoint(IPAddress.Loopback, port));
            server.Listen(10);
            server.BeginAccept(AcceptSocket, null);
        }

        public int Port => port;

        private int port;
        private EagleApplication app;
        private Socket server;

        private void AcceptSocket(IAsyncResult ar)
        {
            //Get socket
            Socket sock = server.EndAccept(ar);

            //Read data
            ReceiveContext ctx = new ReceiveContext(sock);
            sock.BeginReceive(ctx.Buffer, 0, ctx.Buffer.Length, SocketFlags.None, SocketReceive, ctx);

            //Accept next
            server.BeginAccept(AcceptSocket, null);
        }

        private void SocketReceive(IAsyncResult ar)
        {
            //Get context and finish read
            ReceiveContext ctx = (ReceiveContext)ar.AsyncState;
            int read = ctx.Sock.EndReceive(ar);

            //If we read nothing, disconnect
            if (read == 0)
            {
                ctx.Sock.Close();
                return;
            }

            //Deserialize
            JObject message = JsonConvert.DeserializeObject<JObject>(Encoding.UTF8.GetString(ctx.Buffer, 0, read));
            string opcode = (string)message["opcode"];
            JObject payload = (JObject)message["payload"];

            //Switch on opcode
            switch (opcode)
            {
                case "INSTALL":
                    using (var edit = app.Configure())
                        edit.InstallPlugin((string)payload["filename"]);
                    break;
            }

            //Receive next
            try
            {
                ctx.Sock.BeginReceive(ctx.Buffer, 0, ctx.Buffer.Length, SocketFlags.None, SocketReceive, ctx);
            } catch
            {
                //Ignore...
            }
        }

        class ReceiveContext
        {
            public ReceiveContext(Socket sock)
            {
                Sock = sock;
                Buffer = new byte[8192];
            }

            public Socket Sock { get; private set; }
            public byte[] Buffer { get; private set; }
        }
    }
}
