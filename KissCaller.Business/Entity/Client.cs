using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using KissCaller.Business.Comm;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using log4net;

namespace KissCaller.Business.Entity
{
    public partial class Client
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Client).Name);

        [ScriptIgnore]
        [XmlIgnore]
        public BsonObjectId _id = new ObjectId();
        public string ClientIpAddress = string.Empty;
        public string ClientSessionAspNetId = string.Empty;
        public string ClientSessionNodeId = string.Empty;
        public string ClientTwilioClientToken = string.Empty;
        public string ClientTwilioClientId = string.Empty;
        public ClientStateEnum ClientState = ClientStateEnum.Offline;
        public DateTime ClientDateUpdate = DateTime.Now;
        public DateTime ClientDateCreate = DateTime.Now;

        [ScriptIgnore][XmlIgnore]
        public MongoDatabase MongoDb = null;

        public enum ClientStateEnum
        {
            Offline = 0,
            Available = 1,
            Busy = 2
        }

        public Client()
        {
            MongoDb = Helper.MongoDb.GetDatabase();
        }

        public Client(string mongoDbConnStr, string mongoDbName)
        {
            MongoDb = Helper.MongoDb.GetDatabase(mongoDbConnStr, mongoDbName);
        }

        public Client(MongoDatabase mongoDb)
        {
            MongoDb = mongoDb;
        }

        //create/update a client object based on a client event received from the client
        public static Client CreateFromClientEvent(ClientEvent clientEvent, ClientStateEnum clientState)
        {
            if (log.IsDebugEnabled) { log.Debug("CreateFromClientEvent.ClientEvent." + (clientEvent == null ? "null" : clientEvent.ToJsonString())); }

            Client client = null;

            if (clientEvent != null &&
                clientEvent.ClientData != null)
            {
                MongoDatabase mongoDb = Helper.MongoDb.GetDatabase();

                //try to find existing 
                client = Client.FindOneBySessionNodeId(mongoDb, clientEvent.SessionNodeId);
                if (client == null)
                    client = Client.FindOneBySessionAspNetId(mongoDb, clientEvent.ClientData.SessionAspNetId);
                if (client == null)
                    client = new Client();
            
                //upsert the client for future use
                client.ClientSessionNodeId = clientEvent.SessionNodeId;
                client.ClientSessionAspNetId = clientEvent.ClientData.SessionAspNetId;
                client.ClientState = clientState;
                client.Save(mongoDb);
            }

            if (log.IsDebugEnabled) { log.Debug("CreateFromClientEvent.Client." + (client == null ? "null" : client.ToJsonString())); }

            return client;
        }

        public string ToJsonString()
        {
            return Helper.Json.Serialize<Client>(this);
        }

        public static Client FromJsonString(string jsonString)
        {
            return Helper.Json.Deserialise<Client>(jsonString);
        }

    }
}