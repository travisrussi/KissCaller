using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using log4net;

namespace KissCaller.Business.Comm
{
    public partial class ClientResponse
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClientResponse).Name);
        
        private string _SessionNodeId;
        public string SessionNodeId
        {
            get { return _SessionNodeId; }
            set { _SessionNodeId = value; }
        }

        private ClientResponseData _ClientData;
        public ClientResponseData ClientData
        {
            get { return _ClientData; }
            set { _ClientData = value; }
        }

        public string ToJsonString()
        {
            return Helper.Json.Serialize<ClientResponse>(this);
        }

        public static ClientResponse FromJsonString(string jsonString)
            {
                return Helper.Json.Deserialise<ClientResponse>(jsonString);
            }

        public void HandleResponse()
        {
            //handing incoming call response from client
            if (ClientData != null &&
                ClientData.EventName == "incomingcall")
            {
                switch (ClientData.EventResponse)
                {
                    case "answer":
                    case "accept":
                        TwilioComm.TwilioResponse.InitiateCallAnswer(ClientData.Call);
                        break;
                    case "transfer":
                        Entity.Call call = null;

                        //find the existing call from the log
                        if (ClientData.Call != null &&
                            !string.IsNullOrEmpty(ClientData.Call.TransferTo))
                        {
                            call = Entity.Call.FindOneByTwilioId(ClientData.Call.CallTwilioId);
                        }

                        //only transfer if the call was found
                        if (call != null)
                        {
                            //update the call with the transfer to number
                            call.TransferTo = ClientData.Call.TransferTo;
                            call.Save();

                            TwilioComm.TwilioResponse.InitiateCallTransfer(ClientData.Call);
                        }
                        else
                        {
                            //if the call couldn't be found, then just send to voicemail
                            TwilioComm.TwilioResponse.InitiateCallVoicemail(ClientData.Call);
                        }
                        break;
                    case "ignore":
                        TwilioComm.TwilioResponse.InitiateCallIgnore(ClientData.Call);
                        break;
                    case "block":
                        TwilioComm.TwilioResponse.InitiateCallBlock(ClientData.Call);
                        break;
                    case "disconnect":
                        //try to notify called client the call was disconnected
                        ClientRequest.SendEventDisconnect(ClientData.CallTo);
                        break;
                    default: //by default, send calls to voicemail
                        TwilioComm.TwilioResponse.InitiateCallVoicemail(ClientData.Call);
                        break;
                }
            }
            else
            {
                if (ClientData == null)
                    if (log.IsDebugEnabled) { log.Debug("HandleResponse.ClientData.null"); }
                else
                    if (log.IsDebugEnabled) { log.Debug("HandleResponse.ClientData." + (ClientData == null ? "null" : ClientData.ToJsonString())); }
            }
        }

        public class ClientResponseData
        {
            private static readonly ILog log = LogManager.GetLogger(typeof(ClientResponseData).Name);

            private string _EventName;
            public string EventName
            {
                get { return _EventName; }
                set { _EventName = value; }
            }

            private string _EventResponse;
            public string EventResponse
            {
                get { return _EventResponse; }
                set { _EventResponse = value; }
            }

            private Entity.CallBase _Call;
            public Entity.CallBase Call
            {
                get { return _Call; }
                set { _Call = value; }
            }

            private string _CallTo;
            public string CallTo
            {
                get { return _CallTo; }
                set { _CallTo = value; }
            }

            public string ToJsonString()
            {
                return Helper.Json.Serialize<ClientResponseData>(this);
            }

            public static ClientResponseData FromJsonString(string jsonString)
            {
                return Helper.Json.Deserialise<ClientResponseData>(jsonString);
            }

        }
    }
}