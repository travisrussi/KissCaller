using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;
using log4net;

namespace KissCaller.Business.Helper
{
    public class Json
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Json).Name);

        public static T Deserialise<T>(string json)
        {

            if (!string.IsNullOrEmpty(json))
            {
                if (json.StartsWith("\"") &&
                    typeof(T) != typeof(object))
                {
                    using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(object));
                        json = serializer.ReadObject(ms).ToString();
                    }
                }

                try
                {
                    T obj = Activator.CreateInstance<T>();
                    using (MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                    {
                        DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
                        obj = (T)serializer.ReadObject(ms);
                    }

                    return obj;
                }
                catch (Exception ex)
                {
                    //throw ex;
                }
            }
            return default(T);
        }

        public static string Serialize<T>(T obj)
        {
            string sRet = string.Empty;
            JavaScriptSerializer js = new JavaScriptSerializer();
            //js.MaxJsonLength = 2147483644;

            try
            {
                sRet = js.Serialize(obj);
            }
            catch (Exception ex)
            {
                //throw ex;
            }

            return sRet;
        }

        // returns the number of milliseconds since Jan 1, 1970 (useful for converting C# dates to JS dates)
        public static string ConvertDateToJsEpochDate(DateTime dt)
        {
            DateTime d1 = new DateTime(1970, 1, 1);
            DateTime d2 = dt.ToUniversalTime();
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return ts.TotalMilliseconds.ToString("###############");
        }

        public static DateTime ConvertJsEpochDateToDate(string epochDate)
        {
            DateTime dt = DateTime.MinValue;

            try
            {
                double epochDateInSeconds = 0;
                double.TryParse(epochDate, out epochDateInSeconds);
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                dt = epoch.AddSeconds(epochDateInSeconds);
            }
            catch (Exception ex)
            { }

            return dt;
        }

    }
}