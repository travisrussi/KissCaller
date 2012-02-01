using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using KissCaller.Business.Comm;
using MongoDB.Bson;

namespace KissCaller.Business.Entity
{
    public partial class Client
    {
        public Client Save(MongoDatabase mongoDb)
        {
            MongoDb = mongoDb;
            return Save();
        }

        public Client Save()
        {
            return Save(MongoDb, this);
        }

        public static Client Save(Client client)
        {
            MongoDatabase mongoDb = Helper.MongoDb.GetDatabase();
            return Save(mongoDb, client);
        }
        
        public static Client Save(MongoDatabase mongoDb, Client client)
        {
            if (mongoDb == null ||
                client == null)
            {
                return null;
            }

            if (mongoDb.Server == null)
                mongoDb = Helper.MongoDb.GetDatabase();

            try
            {
                if (client.ClientDateCreate == null)
                    client.ClientDateCreate = DateTime.Now;
                client.ClientDateUpdate = DateTime.Now;

                var clientCol = mongoDb.GetCollection<Client>("Client");
                clientCol.Save(client);
            }
            catch (Exception ex)
            {
                if (log.IsDebugEnabled) { log.Error("Save.Client." + (client == null ? "null" : client.ToJsonString()), ex); }
            
                throw ex;
                return null; //"Error: unable to Client.FindOneBySessionAspNetId for " + sessionAspNetId;
            }

            if (log.IsDebugEnabled) { log.Debug("Save.Client." + (client == null ? "null" : client.ToJsonString())); }
            
            return client;
        }

        public static Client FindOneByClientId(BsonObjectId clientId)
        {
            MongoDatabase mongoDb = Helper.MongoDb.GetDatabase();
            return FindOneByClientId(mongoDb, clientId);
        }

        public static Client FindOneByClientId(MongoDatabase mongoDb, BsonObjectId clientId)
        {
            if (mongoDb == null ||
                clientId == null)
            {
                return null;
            }

            if (mongoDb.Server == null)
                mongoDb = Helper.MongoDb.GetDatabase();

            Client client = null;

            try
            {
                var clientCol = mongoDb.GetCollection<Client>("Client");
                var clientQuery = Query.EQ("_id", clientId);
                client = clientCol.FindOne(clientQuery);
            }
            catch (Exception ex)
            {
                if (log.IsDebugEnabled) { log.Error("FindOneByClientId.Client." + (client == null ? "null" : client.ToJsonString()), ex); }
                
                throw ex;
                return null; // "Error: unable to Client.FindOneByClientId for " + ClientId;
            }

            if (log.IsDebugEnabled) { log.Debug("FindOneByClientId.Client." + (client == null ? "null" : client.ToJsonString())); }

            return client;
        }

        public static Client FindOneBySessionAspNetId(string sessionAspNetId)
        {
            MongoDatabase mongoDb = Helper.MongoDb.GetDatabase();
            return FindOneBySessionAspNetId(mongoDb, sessionAspNetId);
        }

        public static Client FindOneBySessionAspNetId(MongoDatabase mongoDb, string sessionAspNetId)
        {
            if (mongoDb == null ||
                string.IsNullOrEmpty(sessionAspNetId))
            {
                return null;
            }

            if (mongoDb.Server == null)
                mongoDb = Helper.MongoDb.GetDatabase();

            Client client = null;

            try
            {
                var clientCol = mongoDb.GetCollection<Client>("Client");
                var clientQuery = Query.EQ("ClientSessionAspNetId", sessionAspNetId);
                client = clientCol.FindOne(clientQuery);
            }
            catch (Exception ex)
            {
                if (log.IsDebugEnabled) { log.Error("FindOneBySessionAspNetId.Client." + (client == null ? "null" : client.ToJsonString()), ex); }

                throw ex;
                return null; // "Error: unable to Client.FindOneBySessionAspNetId for " + sessionAspNetId;
            }

            if (log.IsDebugEnabled) { log.Debug("FindOneBySessionAspNetId.Client." + (client == null ? "null" : client.ToJsonString())); }

            return client;
        }

        public static Client FindOneBySessionNodeId(string sessionNodeId)
        {
            MongoDatabase mongoDb = Helper.MongoDb.GetDatabase();
            return FindOneBySessionNodeId(mongoDb, sessionNodeId);
        }

        public static Client FindOneBySessionNodeId(MongoDatabase mongoDb, string sessionNodeId)
        {
            if (mongoDb == null ||
                string.IsNullOrEmpty(sessionNodeId))
            {
                return null;
            }

            if (mongoDb.Server == null)
                mongoDb = Helper.MongoDb.GetDatabase();

            Client client = null;

            try
            {
                var clientCol = mongoDb.GetCollection<Client>("Client");
                var clientQuery = Query.EQ("ClientSessionNodeId", sessionNodeId);
                client = clientCol.FindOne(clientQuery);
            }
            catch (Exception ex)
            {
                if (log.IsDebugEnabled) { log.Error("FindOneBySessionNodeId.Client." + (client == null ? "null" : client.ToJsonString()), ex); }
                
                throw ex;
                return null; // "Error: unable to Client.FindOneBySessionNodeId for " + sessionNodeId;
            }

            if (log.IsDebugEnabled) { log.Debug("FindOneBySessionNodeId.Client." + (client == null ? "null" : client.ToJsonString())); }

            return client;
        }

        public static Client FindOneByTwilioClientId(string twilioClientId)
        {
            MongoDatabase mongoDb = Helper.MongoDb.GetDatabase();
            return FindOneByTwilioClientId(mongoDb, twilioClientId);
        }

        public static Client FindOneByTwilioClientId(MongoDatabase mongoDb, string twilioClientId)
        {
            if (mongoDb == null ||
                string.IsNullOrEmpty(twilioClientId))
            {
                return null;
            }

            if (mongoDb.Server == null)
                mongoDb = Helper.MongoDb.GetDatabase();

            Client client = null;

            try
            {
                //twilio prepends 'client:' to some clientIds
                twilioClientId = twilioClientId.Replace("client:", "");

                var clientCol = mongoDb.GetCollection<Client>("Client");
                var clientQuery = Query.EQ("ClientTwilioClientId", twilioClientId);
                client = clientCol.FindOne(clientQuery);
            }
            catch (Exception ex)
            {
                if (log.IsDebugEnabled) { log.Error("FindOneByTwilioClientId.Client." + (client == null ? "null" : client.ToJsonString()), ex); }
                
                throw ex;
                return null; // "Error: unable to Client.FindOneByTwilioClientId for " + twilioClientId;
            }

            if (log.IsDebugEnabled) { log.Debug("FindOneByTwilioClientId.Client." + (client == null ? "null" : client.ToJsonString())); }

            return client;
        }

        public static Client FindOneByIpAddress(string ipAddress)
        {
            MongoDatabase mongoDb = Helper.MongoDb.GetDatabase();
            return FindOneByIpAddress(mongoDb, ipAddress);
        }
        
        public static Client FindOneByIpAddress(MongoDatabase mongoDb, string ipAddress)
        {
            if (mongoDb == null ||
                string.IsNullOrEmpty(ipAddress))
            {
                return null;
            }

            if (mongoDb.Server == null)
                mongoDb = Helper.MongoDb.GetDatabase();

            Client client = null;

            try
            {
                var clientCol = mongoDb.GetCollection<Client>("Client");
                var clientQuery = Query.EQ("ClientIpAddress", ipAddress);
                client = clientCol.FindOne(clientQuery);
            }
            catch (Exception ex)
            {
                if (log.IsDebugEnabled) { log.Error("FindOneByIpAddress.Client." + (client == null ? "null" : client.ToJsonString()), ex); }
                
                throw ex;
                return null; // "Error: unable to Client.FindOneByIpAddress for " + ipAddress;
            }

            if (log.IsDebugEnabled) { log.Debug("FindOneByIpAddress.Client." + (client == null ? "null" : client.ToJsonString())); }

            return client;
        }

    }
}