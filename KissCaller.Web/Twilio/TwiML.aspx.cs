using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using KissCaller.Business.Comm;
using System.Threading;
using log4net;

namespace KissCaller.Web.Twilio
{
    public partial class TwiML : System.Web.UI.Page
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TwiML).Name);

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                TwilioComm.TwilioResponse resp = null;
                Business.Entity.Client client = null;
                Business.Entity.Call call = null;

                //parse the incoming request
                TwilioComm.TwilioRequestVoice request = TwilioComm.TwilioRequestVoice.GetFromHttpContext(HttpContext.Current);
                
                if (log.IsDebugEnabled) { log.Debug("Page_Load.RequestReceived.request." + request.ToJsonString()); }
                
                if (request != null &&
                    client == null &&
                    !string.IsNullOrEmpty(request.To))
                {
                    //find client by request to
                    client = Business.Entity.Client.FindOneByTwilioClientId(request.To);
                }

                if (request != null &&
                    call == null &&
                    !string.IsNullOrEmpty(request.CallSid))
                {
                    //find call by twilio call id
                    call = Business.Entity.Call.FindOneByTwilioId(request.CallSid);
                }

                if (client == null &&
                    call != null &&
                    call.ClientToId != null)
                {
                    //find client by kisscaller call id
                    client = Business.Entity.Client.FindOneByClientId(call.ClientToId);
                }
                
                //check if action was specificed; used for callback functions from twilio
                if (resp == null &&
                    client != null &&
                    call != null &&
                    !string.IsNullOrEmpty(Request.QueryString["action"]))
                {
                    if (log.IsDebugEnabled) { log.Debug("Page_Load.RequestReceived.action." + Request.QueryString["action"]); }
                
                    switch (Request.QueryString["action"])
                    {
                        case "answer":
                        case "accept":
                            resp = TwilioComm.TwilioResponse.GetResponseAnswer(request, client, call);
                            break;
                        case "transfer":
                            resp = TwilioComm.TwilioResponse.GetResponseTransfer(request, client, call);
                            break;
                        case "ignore":
                            resp = TwilioComm.TwilioResponse.GetResponseIgnore(request, client, call);
                            break;
                        case "block":
                            resp = TwilioComm.TwilioResponse.GetResponseBlock(request, client, call);
                            break;
                        case "voicemail":
                            resp = TwilioComm.TwilioResponse.GetResponseVoicemail(request, client, call);
                            break;
                    }
                }

                if (resp == null &&
                    client != null)
                {
                    //get TwilioResponse based on the incoming request
                    resp = TwilioComm.TwilioResponse.GetResponse(request, client, call);
                }
                
                if (resp == null)
                {
                    //default to unknown client response
                    resp = TwilioComm.TwilioResponse.GetResponseUnknownClient(request, call);
                }
                
                //log the call
                if (call == null)
                    call = Business.Entity.Call.CreateFromTwilioRequest(request, client, true);

                //write the xml response
                var responseXml = resp.ToString();

                if (log.IsDebugEnabled) { log.Debug("Page_Load.RequestReceived.response." + responseXml); }
                
                Response.Clear();
                Response.ContentType = "text/xml";
                Response.Write(responseXml);
                Response.End();

            }
            
        }
    }

}