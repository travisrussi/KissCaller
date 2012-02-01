using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using log4net;

namespace KissCaller.Business.Comm
{
    public class ClientEvent
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(ClientEvent).Name);

        private string _SessionNodeId;
        public string SessionNodeId
        {
            get { return _SessionNodeId; }
            set { _SessionNodeId = value; }
        }

        private ClientEventData _ClientData;
        public ClientEventData ClientData
        {
            get { return _ClientData; }
            set { _ClientData = value; }
        }

        public string ToJsonString()
        {
            return KissCaller.Business.Helper.Json.Serialize<ClientEvent>(this);
        }

        public static ClientEvent FromJsonString(string jsonString)
        {
            return KissCaller.Business.Helper.Json.Deserialise<ClientEvent>(jsonString);
        }

        public class ClientEventData
        {
            private static readonly ILog log = LogManager.GetLogger(typeof(ClientEventData).Name);

            private string _EventName;
            public string EventName
            {
                get { return _EventName; }
                set { _EventName = value; }
            }

            private string _SessionAspNetId;
            public string SessionAspNetId
            {
                get { return _SessionAspNetId; }
                set { _SessionAspNetId = value; }
            }

        }
    }

}