using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Http;
using System.Collections.Specialized;
using System.Net;
using System.Threading.Tasks;
using System.Configuration;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using log4net;

namespace KissCaller.Business.Comm
{
    public static partial class TwilioComm
    {

        public partial class TwilioResponse : Twilio.TwiML.TwilioResponse
        {
            private static readonly ILog log = LogManager.GetLogger(typeof(TwilioResponse).Name);

            [ScriptIgnore][XmlIgnore]
            public TwilioSettings Settings = new TwilioSettings();

            public TwilioResponse()
            {
            }

            public TwilioResponse(bool setDefaultErrorMessage)
            {
                if (setDefaultErrorMessage)
                {
                    //set a generic error message in case a response was not specified
                    this.Element.RemoveAll();
                    this.Pause(1);
                    this.Say("We're sorry.  You're call could not be completed as dialed.  Please try again later.");
                    this.Pause(1);
                }
            }

            //initiate the request to answer the call to be sent to twilio server with appropriate callback url
            public static void InitiateCallAnswer(Entity.CallBase call)
            {
                if (log.IsDebugEnabled) { log.Debug("InitiateCallAnswer.call. " + call.ToJsonString()); }

                var redirectUrl = string.Format("?action={0}&record={1}", "answer", call.CallRecord);
                InitiateCallModify(call, redirectUrl);
            }

            //initiate the request to transfer the call to be sent to twilio server with appropriate callback url
            public static void InitiateCallTransfer(Entity.CallBase call)
            {
                if (call == null ||
                    string.IsNullOrEmpty(call.TransferTo))
                {
                    if (call == null)
                        if (log.IsDebugEnabled) { log.Error("InitiateCallTransfer.call.null"); }
                    else
                        if (log.IsDebugEnabled) { log.Error("InitiateCallTransfer.call. " + call.ToJsonString()); }

                    return;
                }

                if (log.IsDebugEnabled) { log.Debug("InitiateCallTransfer.call." + (call == null ? "null" : call.ToJsonString())); }

                var redirectUrl = string.Format("?action={0}&to={1}", "transfer", call.TransferTo);
                InitiateCallModify(call, redirectUrl);
            }

            //initiate the request to ignore the call to be sent to twilio server with appropriate callback url
            public static void InitiateCallIgnore(Entity.CallBase call)
            {
                if (log.IsDebugEnabled) { log.Debug("InitiateCallIgnore.call." + (call == null ? "null" : call.ToJsonString())); }

                var redirectUrl = string.Format("?action={0}", "ignore");
                InitiateCallModify(call, redirectUrl);
            }

            //initiate the request to block the call to be sent to twilio server with appropriate callback url
            public static void InitiateCallBlock(Entity.CallBase call)
            {
                if (log.IsDebugEnabled) { log.Debug("InitiateCallBlock.call." + (call == null ? "null" : call.ToJsonString())); }
                
                var redirectUrl = string.Format("?action={0}", "block");
                InitiateCallModify(call, redirectUrl);
            }

            //initiate the request to send the call to voicemail to be sent to twilio server with appropriate callback url
            public static void InitiateCallVoicemail(Entity.CallBase call)
            {
                if (log.IsDebugEnabled) { log.Debug("InitiateCallVoicemail.call." + (call == null ? "null" : call.ToJsonString())); }
                
                var redirectUrl = string.Format("?action={0}", "voicemail");
                InitiateCallModify(call, redirectUrl);
            }

            //initiate the request to modify the call to be sent to twilio server with appropriate callback url
            private static void InitiateCallModify(Entity.CallBase call, string redirectUrl)
            {
                if (call == null ||
                    string.IsNullOrEmpty(call.CallTwilioId) ||
                    string.IsNullOrEmpty(redirectUrl))
                {
                    return;
                }

                TwilioSettings twilioSettings = new TwilioSettings();

                redirectUrl = string.Format("{0}{1}", twilioSettings.TwilioTwiMLCallbackUrl, redirectUrl);
                var postUrl = string.Format("{0}/Accounts/{1}/Calls/{2}", twilioSettings.TwilioApiVersion, twilioSettings.TwilioAccountSid, call.CallTwilioId);

                if (log.IsDebugEnabled) { log.Debug("InitiateCallModify.postUrl." + postUrl); }

                using (HttpClient httpClient = new HttpClient("https://api.twilio.com/"))
                {
                    HttpUrlEncodedForm frm = new HttpUrlEncodedForm();

                    //add the form fields that twilio expects
                    frm.Add("Method", "POST");
                    frm.Add("Url", redirectUrl);

                    //set the security credentials for the http post
                    httpClient.TransportSettings.Credentials = new NetworkCredential(twilioSettings.TwilioAccountSid, twilioSettings.TwilioAuthToken);

                    //send request to twilio
                    using (HttpResponseMessage httpResp = httpClient.Post(postUrl, frm.CreateHttpContent()))
                    {
                        //don't need to do anything with the response of the http post
                        //twilio sends the response in a seperate http request to the callback url
                    }
                }
            }

            //create a TwiML response based on the twilio request received
            public static TwilioResponse GetResponse(TwilioRequestVoice request, Entity.Client client, Entity.Call call)
            {
                TwilioResponse resp = new TwilioResponse();

                //TODO: check database to see if the client has blocked all calls from this caller
                //is caller blocked?
                if (request.From == "client:blocked")
                {
                    if (log.IsDebugEnabled) { log.Debug("GetResponse.Blocked.request." + (request == null ? "null" : request.ToJsonString())); }

                    resp = GetResponseBlock(request, client, call);
                }
                //is client available?
                else if (client == null ||
                         client.ClientState != Entity.Client.ClientStateEnum.Available)
                {
                    if (log.IsDebugEnabled) { log.Debug("GetResponse.NotAvailable.request." + (request == null ? "null" : request.ToJsonString())); }

                    resp = GetResponseVoicemail(request, client, call);
                }
                //ask client what to do
                else
                {
                    if (log.IsDebugEnabled) { log.Debug("GetResponse.Ask.request." + (request == null ? "null" : request.ToJsonString())); }

                    //create a call object to be sent to the client for review
                    Entity.CallBase callBase = Entity.CallBase.CreateFromTwilioRequest(request);

                    //send request to client async
                    Task.Factory.StartNew(() => {
                        ClientRequest.SendEvent(client.ClientSessionNodeId, ClientRequest.RequestEventEnum.callincoming, callBase);
                    });

                    //for now, respond with the default ringing response
                    resp = GetResponseRing(request, client, call);
                }

                return resp;
            }

            //create a default ringing TwiML response
            public static TwilioResponse GetResponseRing(TwilioRequestVoice request, Entity.Client client, Entity.Call call)
            {
                TwilioResponse resp = new TwilioResponse();

                //is client available?
                if (client == null ||
                    client.ClientState != Entity.Client.ClientStateEnum.Available)
                {
                    if (log.IsDebugEnabled) { log.Debug("GetResponseRing.Unavailable.request." + (request == null ? "null" : request.ToJsonString())); }

                    //transfer to unavailable voicemail
                    return GetResponseVoicemail(request, client, call);
                }
                else
                {
                    if (log.IsDebugEnabled) { log.Debug("GetResponseRing.request." + (request == null ? "null" : request.ToJsonString())); }

                    //TODO: find a ring MP3 file to play instead of just pausing with silence
                    //definte the default ringing response
                    resp.Pause(1);
                    resp.Say("Asking the person you are calling what they want to do.");
                    resp.Pause(20);

                    //if the call isn't dealt with after the specified time, then transfer the person to voicemail
                    GetResponseVoicemail(resp, request, client, call);
                    
                    //log the current call state
                    if (call != null)
                    {
                        call.CallResult = "ringing";
                        call.Save();
                    }
                }

                return resp;
            }

            //create a TwiML response to answer the call
            public static TwilioResponse GetResponseAnswer(TwilioRequestVoice request, Entity.Client client, Entity.Call call)
            {
                TwilioResponse resp = new TwilioResponse();

                if (client == null ||
                    client.ClientState != Entity.Client.ClientStateEnum.Available)
                {
                    if (log.IsDebugEnabled) { log.Debug("GetResponseAnswer.Unavailable.request." + (request == null ? "null" : request.ToJsonString())); }
                    
                    //transfer to unavailable voicemail
                    return GetResponseVoicemail(request, client, call);
                }
                else
                {
                    if (log.IsDebugEnabled) { log.Debug("GetResponseAnswer.request." + (request == null ? "null" : request.ToJsonString())); }
                    
                    resp.Pause(1);
                    
                    if (request.CallRecord)
                        GetResponseRecord(resp, request, client, call);

                    resp.Say("Your call is being answered now.");

                    //find the number to dial
                    string toNumber = request.To;
                    if (string.IsNullOrEmpty(toNumber))
                        toNumber = call.ToNumber;

                    //TODO: pull caller id from database
                    //caller ID is required for outbound calls; it must be registered with your twilio account
                    string callerId = ConfigurationManager.AppSettings["DemoCallerId"];;

                    if (toNumber.StartsWith("client:"))
                    {
                        //call is being routed to a twilio client
                        Twilio.TwiML.Client clientToCall = new Twilio.TwiML.Client(toNumber.Replace("client:",""));
                        resp.Dial(clientToCall, new { callerId = callerId });
                    }
                    else
                    {
                        //call is being routed to an external number
                        Twilio.TwiML.Number number = new Twilio.TwiML.Number(toNumber);
                        resp.Dial(number, new { callerId = callerId });
                    }

                    //log the current call state
                    if (call != null)
                    {
                        call.CallResult = "answered by " + request.To;
                        call.Save();
                    }
                }

                return resp;
            }

            //create a TwiML response to transfer the call
            public static TwilioResponse GetResponseTransfer(TwilioRequestVoice request, Entity.Client client, Entity.Call call)
            {
                TwilioResponse resp = new TwilioResponse();

                if (call != null &&
                    !string.IsNullOrEmpty(call.TransferTo))
                {
                    if (log.IsDebugEnabled) { log.Debug("GetResponseTransfer.request." + (request == null ? "null" : request.ToJsonString())); }
                    
                    if (request.CallRecord)
                        GetResponseRecord(resp, request, client, call);

                    resp.Pause(1);
                    resp.Say("You are being transferred now.");

                    //TODO: pull caller id from database
                    //caller ID is required for outbound calls; it must be registered with your twilio account
                    string callerId = ConfigurationManager.AppSettings["DemoCallerId"];

                    Twilio.TwiML.Number number = new Twilio.TwiML.Number(call.TransferTo);
                    resp.Dial(number, new { callerId = callerId });

                    //update call result
                    if (call != null)
                    {
                        call.CallResult = "transfered to " + call.TransferTo;
                        call.Save();
                    }
                }
                else
                {
                    if (log.IsDebugEnabled) { log.Error("GetResponseTransfer.NoTransferNumber.request." + (request == null ? "null" : request.ToJsonString())); }
                    
                    //no transfer number, send to voicemail
                    resp = GetResponseVoicemail(request, client, call);
                }

                return resp;
            }

            //create a TwiML response to send the call to voicemail
            public static TwilioResponse GetResponseVoicemail(TwilioRequestVoice request, Entity.Client client, Entity.Call call)
            {
                return GetResponseVoicemail(new TwilioResponse(), request, client, call);
            }

            //create a TwiML response to send the call to voicemail; appends the response to an existing response
            public static TwilioResponse GetResponseVoicemail(TwilioResponse resp, TwilioRequestVoice request, Entity.Client client, Entity.Call call)
            {
                if (log.IsDebugEnabled) { log.Debug("GetResponseVoicemail.request." + (request == null ? "null" : request.ToJsonString())); }
                
                //TODO: create valid voicemail box dial plan
                resp.Pause(1);
                resp.Say("You are being transferred to voicemail, but it's not setup yet.  Goodbye.");
                resp.Pause(1);
                resp.Hangup();

                //log the current call state
                if (call != null)
                {
                    call.CallResult = "sent to voicemail";
                    call.Save();
                }

                return resp;
            }

            //create a TwiML response to block the call
            public static TwilioResponse GetResponseBlock(TwilioRequestVoice request, Entity.Client client, Entity.Call call)
            {
                if (log.IsDebugEnabled) { log.Debug("GetResponseBlock.request." + (request == null ? "null" : request.ToJsonString())); }
                
                TwilioResponse resp = new TwilioResponse();

                //TODO: create a blocked voicemail box to send the call to
                resp.Pause(1);
                resp.Say("Your calls are blocked.");
                resp.Pause(1);
                resp.Hangup();

                //log the current call state
                if (call != null)
                {
                    call.CallResult = "call was blocked";
                    call.Save();
                }

                return resp;
            }

            //create a TwiML response to ignore the call
            public static TwilioResponse GetResponseIgnore(TwilioRequestVoice request, Entity.Client client, Entity.Call call)
            {
                if (log.IsDebugEnabled) { log.Debug("GetResponseIgnore.request." + (request == null ? "null" : request.ToJsonString())); }
                
                TwilioResponse resp = new TwilioResponse();

                //TODO: create an ignored voicemail box to send the call to
                resp.Pause(1);
                resp.Say("Your call was ignored.");
                resp.Pause(1);
                resp.Hangup();

                //log the current call state
                if (call != null)
                {
                    call.CallResult = "call was ignored";
                    call.Save();
                }

                return resp;
            }

            //create a TwiML response to notify the caller that the call is being recorded; appends the response to an existing response
            public static TwilioResponse GetResponseRecord(TwilioResponse resp, TwilioRequestVoice request, Entity.Client client, Entity.Call call)
            {
                if (log.IsDebugEnabled) { log.Debug("GetResponseRecord.request." + (request == null ? "null" : request.ToJsonString())); }
                
                resp.Say("This call may be recorded for quality assurance.");
                resp.Record();

                //log the current call state
                if (call != null)
                {
                    call.CallRecord = true;
                    call.Save();
                }

                return resp;
            }

            //create a TwiML response for calls to an unknown client
            public static TwilioResponse GetResponseUnknownClient(TwilioRequestVoice request, Entity.Call call)
            {
                if (log.IsDebugEnabled) { log.Debug("GetResponseUnknownClient.request." + (request == null ? "null" : request.ToJsonString())); }
                
                TwilioResponse resp = new TwilioResponse();

                resp.Pause(1);
                resp.Say("We're sorry.  The person you are trying to reach could not be found.");
                resp.Pause(1);
                resp.Hangup();

                //log the current call state
                if (call != null)
                {
                    call.CallResult = "unknown client";
                    call.Save();
                }

                return resp;
            }

        }

    }
}