using EagleWeb.Common;
using EagleWeb.Core.Auth;
using EagleWeb.Core.Web.Util;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EagleWeb.Core.Web.WS
{
    public abstract class EagleBaseConnection
    {
        public EagleBaseConnection(EagleAccount account)
        {
            this.account = account;
        }

        private readonly EagleAccount account;
        private readonly PostingQueue<OutgoingMessage> outgoing = new PostingQueue<OutgoingMessage>();

        private const int DEFAULT_RECEIVE_BUFFER_SIZE = 2048;

        public EagleAccount Account => account;

        public async Task RunAsync(WebSocket sock)
        {
            //Create buffers
            byte[] receiveBuffer = new byte[DEFAULT_RECEIVE_BUFFER_SIZE];
            int receiveBufferUse = 0;

            //Start both tasks
            Task<WebSocketReceiveResult> taskReceive = sock.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, receiveBufferUse, receiveBuffer.Length - receiveBufferUse), CancellationToken.None);
            Task<OutgoingMessage> taskSend = outgoing.ReceiveAsync();

            //Tell the client that they're ready
            ClientReady();

            //Wait for one of these to complete
            while (true)
            {
                Task completed = await Task.WhenAny(taskReceive, taskSend);
                if (completed == taskReceive)
                {
                    //Get the result
                    WebSocketReceiveResult resultR = taskReceive.Result;

                    //Update
                    receiveBufferUse += resultR.Count;

                    //Check if we need to read the next chunk
                    if (!resultR.EndOfMessage)
                    {
                        //Extend the buffer
                        byte[] extended = new byte[receiveBuffer.Length * 2];
                        receiveBuffer.CopyTo(extended, 0);
                        receiveBuffer = extended;
                    } else
                    {
                        //Check if this is the socket closing
                        if (resultR.MessageType == WebSocketMessageType.Close)
                            break;

                        //Allow the client to act
                        ClientReceive(receiveBuffer, receiveBufferUse, resultR.MessageType == WebSocketMessageType.Text);

                        //Make sure the buffer is back to it's default size
                        if (receiveBuffer.Length != DEFAULT_RECEIVE_BUFFER_SIZE)
                            receiveBuffer = new byte[DEFAULT_RECEIVE_BUFFER_SIZE];

                        //Reset
                        receiveBufferUse = 0;
                    }

                    //Read next
                    taskReceive = sock.ReceiveAsync(new ArraySegment<byte>(receiveBuffer, receiveBufferUse, receiveBuffer.Length - receiveBufferUse), CancellationToken.None);
                }
                if (completed == taskSend)
                {
                    //Get the result
                    OutgoingMessage resultR = taskSend.Result;

                    //Send
                    if (resultR.type == WebSocketMessageType.Close)
                    {
                        await sock.CloseAsync(WebSocketCloseStatus.NormalClosure, Encoding.UTF8.GetString(resultR.buffer), CancellationToken.None);
                        break;
                    } else
                    {
                        await sock.SendAsync(new ArraySegment<byte>(resultR.buffer, 0, resultR.buffer.Length), resultR.type, true, CancellationToken.None);
                    }

                    //Start next
                    taskSend = outgoing.ReceiveAsync();
                }
            }

            //Tell client to clean up
            ClientClosed();
        }

        public virtual bool Authenticate(HttpContext e)
        {
            return true;
        }

        public void Send(byte[] data, int index, int count, bool asText)
        {
            //Make a copy of the data
            byte[] payload = new byte[count];
            Array.Copy(data, index, payload, 0, count);

            //Send
            outgoing.Post(new OutgoingMessage
            {
                buffer = payload,
                type = asText ? WebSocketMessageType.Text : WebSocketMessageType.Binary
            });
        }

        public void Send(string msg)
        {
            //Encode message
            byte[] data = Encoding.UTF8.GetBytes(msg);

            //Send
            Send(data, 0, data.Length, true);
        }

        public void Send(JObject msg)
        {
            Send(JsonConvert.SerializeObject(msg));
        }

        protected void Close(string reason)
        {
            outgoing.Post(new OutgoingMessage
            {
                buffer = Encoding.UTF8.GetBytes(reason),
                type = WebSocketMessageType.Close
            });
        }

        protected abstract void ClientReceive(byte[] data, int count, bool asText);
        protected abstract void ClientReady();
        protected abstract void ClientClosed();

        struct OutgoingMessage
        {
            public byte[] buffer;
            public WebSocketMessageType type;
        }
    }
}
