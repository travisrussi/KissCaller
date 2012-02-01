using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using KissCaller.Business.Comm;
using System.Configuration;

namespace KissCaller.Web.Twilio
{
    public partial class Client : System.Web.UI.Page
    {
        private string _AspNetSessionId = string.Empty;
        public string AspNetSessionId
        {
            get
            {
                if (string.IsNullOrEmpty(_AspNetSessionId) &&
                    Session != null)
                {
                    _AspNetSessionId = Session.SessionID;
                }
                return _AspNetSessionId;
            }
            set
            {
                _AspNetSessionId = value;
            }
        }

        private string _NodeUrl = string.Empty;
        public string NodeUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_NodeUrl))
                {
                    _NodeUrl = ConfigurationManager.AppSettings["NodeUrl"];
                }
                return _NodeUrl;
            }
            set
            {
                _NodeUrl = value;
            }
        }

        private string _TwilioClientId = "jenny";
        public string TwilioClientId
        {
            get
            {
                return _TwilioClientId;
            }
            set
            {
                _TwilioClientId = value;
            }
        }

        private string _TwilioClientToken = string.Empty;
        public string TwilioClientToken
        {
            get
            {
                if (string.IsNullOrEmpty(_TwilioClientToken))
                {
                    _TwilioClientToken = TwilioComm.TwilioRequest.GenerateCapabilityToken(AspNetSessionId, TwilioClientId);
                }

                return _TwilioClientToken;
            }
            set
            {
                _TwilioClientToken = value;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!string.IsNullOrEmpty(Request.QueryString["clientId"]))
                {
                    TwilioClientId = Request.QueryString["clientId"];
                }
            }
        }

    }
}