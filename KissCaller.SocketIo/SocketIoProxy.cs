using System;
using System.Diagnostics;
using System.Threading;
using SocketIOClient;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Text.RegularExpressions;
using KissCaller.Business.Comm;
using log4net;

namespace KissCaller.SocketIo
{
    
	public class SocketIoProxy
	{
        private static readonly ILog log = LogManager.GetLogger(typeof(SocketIoProxy).Name);

        private Client socket;

		public void Start(string nodeServerUrl)
		{
            if (log.IsDebugEnabled) { log.Debug("Start"); }

            Console.WriteLine("Starting SocketIoProxy...");
			socket = new Client(nodeServerUrl); // url to the nodejs / socket.io instance

			socket.Opened += socket_Opened;
			socket.Message += socket_OnMessage;
			socket.SocketConnectionClosed +=socket_SocketConnectionClosed;
			socket.Error += socket_Error;
			
			socket.On("connect", (data) =>
			{
                if (log.IsDebugEnabled) { log.Debug("Start.Connect.data." + (data == null ? "null" : data.JsonEncodedMessage.ToJsonString())); }

                Console.WriteLine("\r\nSocketIoProxy.connect: {0}", data.JsonEncodedMessage.ToJsonString());
			});

            socket.On("clientEventInternal", (data) =>
            {
                ClientEvent clientEvent = null;

                if (data != null &&
                    data.JsonEncodedMessage != null &&
                    data.JsonEncodedMessage.Args != null &&
                    data.JsonEncodedMessage.Args.Length > 0)
                {
                    try
                    {
                        clientEvent = ClientEvent.FromJsonString(data.JsonEncodedMessage.Args[0].ToString());
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                if (clientEvent != null &&
                    clientEvent.ClientData != null)
                {
                    //if it's a connect event, then create the client
                    if (!string.IsNullOrEmpty(clientEvent.ClientData.EventName) &&
                        clientEvent.ClientData.EventName.ToLower() == "connect")
                    {
                        if (log.IsDebugEnabled) { log.Debug("Start.clientEventInternal.connect.clientEvent." + (clientEvent == null ? "null" : clientEvent.ToJsonString())); }

                        Business.Entity.Client.CreateFromClientEvent(clientEvent, Business.Entity.Client.ClientStateEnum.Available);
                        //socket.Emit("clientRequestInternal", new { SessionNodeId = clientEvent.SessionNodeId, Color = "Blue" });
                    }
                    else
                    {
                        if (log.IsDebugEnabled) { log.Debug("Start.clientEventInternal." + clientEvent.ClientData.EventName + ".clientEvent." + (clientEvent == null ? "null" : clientEvent.ToJsonString())); }
                    }
                }
                else
                {
                    if (log.IsDebugEnabled) { log.Debug("Start.clientEventInternal.UnableToDeserialize.data." + (data == null ? "null" : data.JsonEncodedMessage.ToJsonString())); }

                    Console.WriteLine("\r\nSocketIoProxy.clientEvent (unable to deserialize): {0}", data.JsonEncodedMessage.ToJsonString());
                }
            });

			socket.On("clientResponseInternal", (data) =>
			{
                ClientResponse clientResponse = null;
                
                if (data != null &&
                    data.JsonEncodedMessage != null &&
                    data.JsonEncodedMessage.Args != null &&
                    data.JsonEncodedMessage.Args.Length > 0)
                {
                    try
                    {
                        clientResponse = ClientResponse.FromJsonString(data.JsonEncodedMessage.Args[0].ToString());
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                if (clientResponse != null &&
                    clientResponse.ClientData != null)
                {
                    if (log.IsDebugEnabled) { log.Debug("Start.clientResponseInternal.HandleResponse.clientResponse." + (clientResponse == null ? "null" : clientResponse.ToJsonString())); }

                    clientResponse.HandleResponse();
                    //socket.Emit("clientRequestInternal", new { SessionNodeId = clientEvent.SessionNodeId, Color = "Blue" });
                }
                else
                {
                    if (log.IsDebugEnabled) { log.Debug("Start.clientResponseInternal.UnableToDeserialize.data." + (data == null ? "null" : data.JsonEncodedMessage.ToJsonString())); }

                    Console.WriteLine("\r\nSocketIoProxy.clientResponse (unable to deserialize): {0}", data.JsonEncodedMessage.ToJsonString());
                }
			});

            socket.On("serverRequestInternal", (data) =>
            {
                Console.WriteLine("\r\nSocketIoProxy.serverRequest: {0}", data.JsonEncodedMessage.ToJsonString());
                
                ClientEvent clientEvent = null;

                if (data != null &&
                    data.JsonEncodedMessage != null &&
                    data.JsonEncodedMessage.Args != null &&
                    data.JsonEncodedMessage.Args.Length > 0)
                {
                    try
                    {
                        clientEvent = ClientEvent.FromJsonString(data.JsonEncodedMessage.Args[0].ToString());
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                if (clientEvent != null)
                {
                    if (log.IsDebugEnabled) { log.Debug("Start.serverRequestInternal.EmitServerRequest.clientEvent." + (clientEvent == null ? "null" : clientEvent.ToJsonString())); }

                    socket.Emit("serverRequest", new { SessionNodeId = clientEvent.SessionNodeId, Color = "Red" });
                }
                else
                {
                    if (log.IsDebugEnabled) { log.Debug("Start.serverRequestInternal.UnableToDeserialize.data." + (data == null ? "null" : data.JsonEncodedMessage.ToJsonString())); }
                }
            });

			socket.Connect();
		}

        public void Close()
        {
            if (this.socket != null)
            {
                socket.Opened -= socket_Opened;
                socket.Message -= socket_OnMessage;
                socket.SocketConnectionClosed -= socket_SocketConnectionClosed;
                socket.Error -= socket_Error;
                this.socket.Dispose(); // close & dispose of socket client
            }
        }

		protected void socket_Error(object sender, ErrorEventArgs e)
		{
            if (log.IsDebugEnabled) { log.Error("socket_Error.eMessage." + e.Message, e.Exception); }

            Console.WriteLine("\r\nSocketIoProxy connection error: ");
			Console.WriteLine(e.Message);
		}

        protected void socket_SocketConnectionClosed(object sender, EventArgs e)
		{
            if (log.IsDebugEnabled) { log.Debug("socket_SocketConnectionClosed"); }

            //Console.WriteLine("\r\nSocketIoProxy connection closed");
		}

        protected void socket_OnMessage(object sender, MessageEventArgs e)
		{
            if (e.Message == null)
            {
                if (log.IsDebugEnabled) { log.Debug("socket_Error.e." + e.ToString()); }

                Console.WriteLine("\r\nSocketIoProxy.Message: {0}", e.ToString());
            }
            else
            {
                if (log.IsDebugEnabled) { log.Debug("socket_Error.eMessage." + e.Message.JsonEncodedMessage.ToJsonString()); }

                Console.WriteLine("\r\nSocketIoProxy.Message: {0} : {1}", e.Message.Event, e.Message.JsonEncodedMessage.ToJsonString());
            }
		}

        protected void socket_Opened(object sender, EventArgs e)
		{
            if (log.IsDebugEnabled) { log.Debug("socket_Opened"); }
            
            //Console.WriteLine("\r\nSocketIoProxy connection opened");
		}

	}
	
}
