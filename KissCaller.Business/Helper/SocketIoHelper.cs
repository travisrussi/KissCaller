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
    public partial class SocketIo
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(SocketIo).Name);

        public static SocketIOClient.Client GetClient()
        {
            return new SocketIOClient.Client(ConfigurationManager.AppSettings["NodeUrl"]);
        }

        public static SocketIOClient.Client GetClient(string serverAddress)
        {
            return new SocketIOClient.Client(serverAddress);
        }
    }
}