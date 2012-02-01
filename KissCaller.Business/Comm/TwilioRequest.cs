using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Http;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;
using System.Xml.Serialization;
using System.Web.Script.Serialization;
using log4net;

namespace KissCaller.Business.Comm
{
    public static partial class TwilioComm
    {
        public partial class TwilioSettings
        {
            private static readonly ILog log = LogManager.GetLogger(typeof(TwilioSettings).Name);

            //common class to store the twilio settings
            public string TwilioAccountSid = ConfigurationManager.AppSettings["TwilioAccountSid"];
            public string TwilioAuthToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
            public string TwilioApplicationSid = ConfigurationManager.AppSettings["TwilioApplicationSid"];
            public string TwilioApiVersion = ConfigurationManager.AppSettings["TwilioApiVersion"];
            public string TwilioTwiMLCallbackUrl = ConfigurationManager.AppSettings["TwilioTwiMLCallbackUrl"];
        }

        public partial class TwilioRequest
        {
            private static readonly ILog log = LogManager.GetLogger(typeof(TwilioRequest).Name);

            [ScriptIgnore]
            [XmlIgnore]
            public TwilioSettings Settings = new TwilioSettings();

            public static string GenerateCapabilityToken(string sessionAspNetId, string twilioClientId)
            {
                TwilioSettings twilioSettings = new TwilioSettings();

                //generate new capability token; used for client-side connectivity directly to twilio servers
                var capability = new Twilio.TwilioCapability(twilioSettings.TwilioAccountSid, twilioSettings.TwilioAuthToken);
                capability.AllowClientIncoming(twilioClientId);
                capability.AllowClientOutgoing(twilioSettings.TwilioApplicationSid);
                string capabilityToken = capability.GenerateToken();

                //try to find existing client
                Entity.Client client = Entity.Client.FindOneByTwilioClientId(twilioClientId);
                if (client == null)
                    client = Entity.Client.FindOneBySessionAspNetId(sessionAspNetId);
                if (client == null)
                    client = new Entity.Client();

                //associate the capability token with the client
                client.ClientSessionAspNetId = sessionAspNetId;
                client.ClientTwilioClientId = twilioClientId;
                client.ClientTwilioClientToken = capabilityToken;
                client.Save();

                if (log.IsDebugEnabled) { log.Debug("GenerateCapabilityToken." + (client == null ? "null" : client.ToJsonString())); }

                return capabilityToken;
            }
        }

        public partial class TwilioRequestVoice : TwilioRequest
        {
            private static readonly ILog log = LogManager.GetLogger(typeof(TwilioRequestVoice).Name);

            public string Caller = null;
            public CallStatusEnum CallStatus = CallStatusEnum.Unknown;
            public string Called = null;
            public string To = null;
            public string CallSid = null;
            public string From = null;
            public string Direction = null;
            public bool CallRecord = false;

            //list of twilio call statuses
            public enum CallStatusEnum
            {
                Queued = 1, //	    The call is ready and waiting in line before going out.
                Ringing = 2, //	    The call is currently ringing.
                InProgress = 3, //	The call was answered and is currently in progress.
                Completed = 4, //	    The call was answered and has ended normally.
                Busy = 5, //	        The caller received a busy signal.
                Failed = 6, //	    The call could not be completed as dialed, most likely because the phone number was non-existent.
                NoAnswer = 7, //	    The call ended without being answered.
                Canceled = 8, //	    The call was canceled via the REST API while queued or ringing.
                Unknown = 9
            }
            
            public TwilioRequestVoice()
            { }

            //parse the twilio request from the httpcontext
            public static TwilioRequestVoice GetFromHttpContext(HttpContext context)
            {
                TwilioRequestVoice request = new TwilioRequestVoice();

                NameValueCollection form = null;
                if (context != null &&
                    context.Request != null)
                {
                    //parse from the request form
                    if (context.Request.RequestType.ToLower() == "post" &&
                        context.Request.Form != null)
                    {
                        form = context.Request.Form;
                    }

                    //parse from the query string
                    if (context.Request.QueryString != null)
                    {
                        if (form == null)
                            form = context.Request.QueryString;

                        //custom parameters only present in query string
                        request.To = context.Request.QueryString["to"];
                        bool callRecord = false;
                        bool.TryParse(context.Request.QueryString["record"], out callRecord);
                        request.CallRecord = callRecord;
                    }
                }

                if (form != null)
                {
                    //set property values for the request object
                    request.Settings.TwilioAccountSid = request.Settings.TwilioAccountSid ?? form["AccountSid"];
                    request.Settings.TwilioApplicationSid = request.Settings.TwilioApplicationSid ?? form["ApplicationSid"];
                    request.Caller = request.Caller ?? form["Caller"];
                    request.CallStatus = GetCallStatus(form["CallStatus"]);
                    request.Called = request.Called ?? form["Called"];
                    request.To = request.To ?? form["To"];
                    request.To = (string.IsNullOrEmpty(request.To) ? form["tocall"] : request.To);
                    request.CallSid = request.CallSid ?? form["CallSid"];
                    request.From = request.From ?? form["From"];
                    request.Direction = request.Direction ?? form["Direction"];
                    request.Settings.TwilioApiVersion = request.Settings.TwilioApiVersion ?? form["ApiVersion"];
                }

                if (log.IsDebugEnabled) { log.Debug("GetFromHttpContext." + (request == null ? "null" : request.ToJsonString())); }

                //ensure the request is valid before returning
                if (!string.IsNullOrEmpty(request.Settings.TwilioAccountSid) &&
                    !string.IsNullOrEmpty(request.CallSid))
                {
                    return request;
                }
                else
                    return null;
            }

            //gets twilio call status from string
            public static CallStatusEnum GetCallStatus(string callStatus)
            {
                CallStatusEnum callStatusEnum = CallStatusEnum.Unknown;

                switch (callStatus)
                {
                    case "queued": //	    The call is ready and waiting in line before going out.
                        callStatusEnum = CallStatusEnum.Queued;
                        break;
                    case "ringing": //	    The call is currently ringing.
                        callStatusEnum = CallStatusEnum.Ringing;
                        break;
                    case "in-progress": //	The call was answered and is currently in progress.
                        callStatusEnum = CallStatusEnum.InProgress;
                        break;
                    case "completed": //	    The call was answered and has ended normally.
                        callStatusEnum = CallStatusEnum.Completed;
                        break;
                    case "busy": //	        The caller received a busy signal.
                        callStatusEnum = CallStatusEnum.Busy;
                        break;
                    case "failed": //	    The call could not be completed as dialed, most likely because the phone number was non-existent.
                        callStatusEnum = CallStatusEnum.Failed;
                        break;
                    case "no-answer": //	    The call ended without being answered.
                        callStatusEnum = CallStatusEnum.NoAnswer;
                        break;
                    case "canceled": //	    The call was canceled via the REST API while queued or ringing.
                        callStatusEnum = CallStatusEnum.Canceled;
                        break;
                }

                return callStatusEnum;
            }

            public string ToJsonString()
            {
                return Helper.Json.Serialize<TwilioRequestVoice>(this);
            }

            public static TwilioRequestVoice FromJsonString(string jsonString)
            {
                return Helper.Json.Deserialise<TwilioRequestVoice>(jsonString);
            }

        }

    }
}