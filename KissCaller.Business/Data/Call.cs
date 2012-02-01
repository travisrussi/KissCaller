using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace KissCaller.Business.Entity
{
    public partial class Call
    {
        public Call Save(MongoDatabase mongoDb)
        {
            MongoDb = mongoDb;
            return Save();
        }

        public Call Save()
        {
            return Save(MongoDb, this);
        }

        public static Call Save(Call Call)
        {
            MongoDatabase mongoDb = Helper.MongoDb.GetDatabase();
            return Save(mongoDb, Call);
        }

        public static Call Save(MongoDatabase mongoDb, Call Call)
        {
            if (mongoDb == null ||
                Call == null)
            {
                return null;
            }
        
            if (mongoDb.Server == null)
                mongoDb = Helper.MongoDb.GetDatabase();

            try
            {
                if (Call.CallDateCreate == null)
                    Call.CallDateCreate = DateTime.Now;
                Call.CallDateUpdate = DateTime.Now;

                var CallCol = mongoDb.GetCollection<Call>("Call");
                CallCol.Save(Call);
            }
            catch (Exception ex)
            {
                if (log.IsDebugEnabled) { log.Error("Business.Call.Save." + (Call == null ? "null" : Call.ToJsonString()), ex); }
            
                throw ex;
                return null; //"Error: unable to Call.FindOneBySessionAspNetId for " + sessionAspNetId;
            }

            if (log.IsDebugEnabled) { log.Debug("Save." + (Call == null ? "null" : Call.ToJsonString())); }

            return Call;
        }

        public static Call FindOneByTwilioId(string TwilioId)
        {
            MongoDatabase mongoDb = Helper.MongoDb.GetDatabase();
            return FindOneByTwilioId(mongoDb, TwilioId);
        }

        public static Call FindOneByTwilioId(MongoDatabase mongoDb, string TwilioId)
        {
            if (mongoDb == null ||
                string.IsNullOrEmpty(TwilioId))
            {
                return null;
            }

            if (mongoDb.Server == null)
                mongoDb = Helper.MongoDb.GetDatabase();

            Call Call = null;

            try
            {
                var CallCol = mongoDb.GetCollection<Call>("Call");
                var CallQuery = Query.EQ("CallTwilioId", TwilioId);
                Call = CallCol.FindOne(CallQuery);
            }
            catch (Exception ex)
            {
                if (log.IsDebugEnabled) { log.Error("Business.Call.FindOneByTwilioId.call." + (Call == null ? "null" : Call.ToJsonString()), ex); }

                throw ex;
                return null; // "Error: unable to Call.FindOneByTwilioId for " + TwilioId;
            }

            if (log.IsDebugEnabled) { log.Debug("FindOneByTwilioId.call." + (Call == null ? "null" : Call.ToJsonString())); }

            return Call;
        }

    }
}