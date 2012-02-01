using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Serialization;
using MongoDB.Driver;
using System.Configuration;
using log4net;

namespace KissCaller.Business.Helper
{
    public partial class MongoDb
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(MongoDb).Name);

        public static MongoDatabase GetDatabase()
        {
            return GetDatabase(ConfigurationManager.AppSettings["MongoDbUrl"], ConfigurationManager.AppSettings["MongoDbName"]);
        }

        public static MongoDatabase GetDatabase(string mongoDbConnStr, string mongoDbName)
        {
            MongoServer server = MongoServer.Create(mongoDbConnStr);
            
            MongoDatabase mongoDb = null;

            try
            {
                mongoDb = server.GetDatabase(mongoDbName);
            }
            catch (Exception ex)
            {
                throw ex;
                return null; // "Error: unable to connect to database";
            }

            return mongoDb;
        }

    }
}