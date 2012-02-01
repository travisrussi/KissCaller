using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using SocketIOClient;
using System.Text;
using log4net;

namespace KissCaller.Business.Comm
{

    public partial class ClientRequest
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClientRequest).Name);

        public string SessionNodeId = string.Empty;
        public string EventName
        {
            get
            {
                return EventNameEnum.ToString();
            }
        }
        public Entity.CallBase Call = new Entity.CallBase();

        [ScriptIgnore]
        [XmlIgnore]
        public RequestEventEnum EventNameEnum = RequestEventEnum.callincoming;
        
        public enum RequestEventEnum
        {
            callincoming = 0,
            calldisconnect = 1
        }

        public static void SendEventDisconnect(string callTo)
        {
            Entity.Client client = Entity.Client.FindOneByTwilioClientId(callTo);
            if (client == null)
                return;

            ClientRequest.SendEvent(client.ClientSessionNodeId, ClientRequest.RequestEventEnum.calldisconnect);
        }

        public static void SendEvent(string sessionNodeId, RequestEventEnum eventName)
        {
            SendEvent(sessionNodeId, eventName, null);
        }

        public static void SendEvent(string sessionNodeId, RequestEventEnum eventName, Entity.CallBase call)
        {
            ClientRequest clientRequest = new ClientRequest();
            clientRequest.SessionNodeId = sessionNodeId;
            clientRequest.EventNameEnum = eventName;
            clientRequest.Call = call;

            SendEvent(clientRequest);
        }

        public static void SendEvent(ClientRequest clientRequest)
        {
            //send request to client via nojejs/socket.io
            using (SocketIOClient.Client socket = Helper.SocketIo.GetClient())
            {
                try
                {
                    //hook event handlers
                    socket.Opened += socket_Opened;
                    socket.Message += socket_OnMessage;
                    socket.SocketConnectionClosed += socket_SocketConnectionClosed;
                    socket.Error += socket_Error;
			
                    //open the socket connections
                    socket.Connect();

                    //ensure a connection has been established
                    int iCnt = 0;
                    while (!socket.IsConnected && iCnt < 5)
                    {
                        Thread.Sleep(50);
                        iCnt++;
                    }

                    if (!socket.IsConnected)
                        if (log.IsDebugEnabled) { log.Debug("SendEvent.SocketError.Unable to connect"); }

                    if (log.IsDebugEnabled) { log.Debug("SendEvent.clientRequestInternal." + (clientRequest == null ? "null" : clientRequest.ToJsonString())); }

                    //send client request
                    socket.Emit("clientRequestInternal", clientRequest);
                }
                finally
                {
                    socket.Close();
                }
            }
        }

        public string ToJsonString()
        {
            return Helper.Json.Serialize<ClientRequest>(this);
        }

        public static ClientRequest FromJsonString(string jsonString)
        {
            return Helper.Json.Deserialise<ClientRequest>(jsonString);
        }

        static void socket_Error(object sender, ErrorEventArgs e)
        {
            if (log.IsDebugEnabled) { log.Error("SendEvent.SocketError.eMessage" + e.Message, e.Exception); }

            //string s = e.Message;
            //Exception ex = e.Exception;
        }

        static void socket_SocketConnectionClosed(object sender, EventArgs e)
        {
            if (log.IsDebugEnabled) { log.Debug("SendEvent.SocketClosed"); }
        }

        static void socket_OnMessage(object sender, MessageEventArgs e)
        {
            if (log.IsDebugEnabled) { log.Debug("SendEvent.SocketMessage." + e.Message.JsonEncodedMessage.ToJsonString()); }
            //string s = e.Message.MessageText;
        }

        static void socket_Opened(object sender, EventArgs e)
        {
            if (log.IsDebugEnabled) { log.Debug("SendEvent.SocketOpened"); }
        }

    }

}