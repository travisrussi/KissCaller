using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using KissCaller.Business.Comm;
using log4net;

namespace KissCaller.Business.Entity
{
    //call object that can be saved to database; but cannot not serialized and sent through node :(
    public partial class Call : ICall
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Call).Name);

        [ScriptIgnore]
        [XmlIgnore]
        public BsonObjectId _id = new ObjectId();
        [ScriptIgnore]
        [XmlIgnore]
        public BsonObjectId ClientToId = null;

        public string CallTwilioId { get; set; }
        public string CallTime { get; set; }
        public string FromNumber { get; set; }
        public string FromName { get; set; }
        public string FromProfileUrl { get; set; }
        public string ToNumber { get; set; }
        public string ToName { get; set; }
        public string TransferTo { get; set; }
        public bool CallRecord { get; set; }
        public string CallResult { get; set; }
        public DateTime CallDateUpdate { get; set; }
        public DateTime CallDateCreate { get; set; }

        [ScriptIgnore]
        [XmlIgnore]
        public MongoDatabase MongoDb = null;

        public Call()
        {
            MongoDb = Helper.MongoDb.GetDatabase();
        }

        public Call(string mongoDbConnStr, string mongoDbName)
        {
            MongoDb = Helper.MongoDb.GetDatabase(mongoDbConnStr, mongoDbName);
        }

        public Call(MongoDatabase mongoDb)
        {
            MongoDb = mongoDb;
        }

        //create a call object from a twilio request
        public static Call CreateFromTwilioRequest(TwilioComm.TwilioRequestVoice twilioRequest)
        {
            if (twilioRequest == null)
                return null;

            Call call = new Call();
            call.FromNumber = twilioRequest.From;
            call.FromName = twilioRequest.From;
            call.CallTwilioId = twilioRequest.CallSid;
            call.ToName = twilioRequest.To;
            call.ToNumber = twilioRequest.To;

            if (log.IsDebugEnabled) { log.Debug("CreateFromTwilioRequest.Call." + (call == null ? "null" : call.ToJsonString())); }

            return call;
        }

        //create a call object from a twilio request and save the call to the database
        public static Call CreateFromTwilioRequest(TwilioComm.TwilioRequestVoice twilioRequest, Client client, bool saveCall)
        {
            if (twilioRequest == null)
                return null;

            return CreateFromTwilioRequest(null, twilioRequest, client, saveCall);
        }

        //create a call object from a twilio request and save the call to the database
        public static Call CreateFromTwilioRequest(MongoDatabase mongoDb, TwilioComm.TwilioRequestVoice twilioRequest, Client client, bool saveCall)
        {
            if (twilioRequest == null)
                return null;

            if (mongoDb == null ||
                mongoDb.Server == null)
            {
                mongoDb = Helper.MongoDb.GetDatabase();
            }

            Call call = Call.CreateFromTwilioRequest(twilioRequest);

            if (client != null)
                call.ClientToId = client._id;

            if (call.CallDateCreate == null)
                call.CallDateCreate = DateTime.Now;
            call.CallDateUpdate = DateTime.Now;

            if (saveCall)
                call.Save();

            if (log.IsDebugEnabled) { log.Debug("CreateFromTwilioRequest.call." + (call == null ? "null" : call.ToJsonString())); }

            return call;
        }

        public string ToJsonString()
        {
            return Helper.Json.Serialize<Call>(this);
        }

        public static Call FromJsonString(string jsonString)
        {
            return Helper.Json.Deserialise<Call>(jsonString);
        }
    }

    //call object that can be serialized and set via node
    public partial class CallBase : ICall
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CallBase).Name);

        public string CallTwilioId { get; set; }
        public string CallTime { get; set; }
        public string FromNumber { get; set; }
        public string FromName { get; set; }
        public string FromProfileUrl { get; set; }
        public string ToNumber { get; set; }
        public string ToName { get; set; }
        public string TransferTo { get; set; }
        public string CallResult { get; set; }
        public bool CallRecord { get; set; }
        public DateTime CallDateUpdate  { get; set; }
        public DateTime CallDateCreate { get; set; }

        //create a call object from a twilio request
        public static CallBase CreateFromTwilioRequest(TwilioComm.TwilioRequestVoice twilioRequest)
        {
            if (twilioRequest == null)
                return null;

            CallBase call = new CallBase();
            call.FromNumber = twilioRequest.From;
            call.FromName = twilioRequest.From;
            call.CallTwilioId = twilioRequest.CallSid;

            if (log.IsDebugEnabled) { log.Debug("CreateFromTwilioRequest.call." + (call == null ? "null" : call.ToJsonString())); }

            return call;
        }

        public string ToJsonString()
        {
            return Helper.Json.Serialize<CallBase>(this);
        }

        public static CallBase FromJsonString(string jsonString)
        {
            return Helper.Json.Deserialise<CallBase>(jsonString);
        }
    }

    //interface to ensure compatability between serializeable callbase and savable call objects
    public interface ICall
    {
        string CallTwilioId { get; set; }
        string CallTime { get; set; }
        string FromNumber { get; set; }
        string FromName { get; set; }
        string FromProfileUrl { get; set; }
        string ToNumber { get; set; }
        string ToName { get; set; }
        string TransferTo { get; set; }
        string CallResult { get; set; }
        bool CallRecord { get; set; }
        DateTime CallDateUpdate { get; set; }
        DateTime CallDateCreate { get; set; }
    }
}