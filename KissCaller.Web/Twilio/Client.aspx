<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Client.aspx.cs" Inherits="KissCaller.Web.Twilio.Client" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>KissCaller - Twilio Client Demo</title>

    <script type="text/javascript" src="http://static.twilio.com/libs/twiliojs/1.0/twilio.js"></script>
    <script src="../js/socket.io/socket.io.js" type="text/javascript"></script>
    <script type="text/javascript" src="http://ajax.googleapis.com/ajax/libs/jquery/1.6.1/jquery.min.js"></script>
    <script src="../js//dump.js" type="text/javascript"></script>

</head>
<body>
    <form id="form1" runat="server">
    <div>
        KissCaller - Twilio Client Demo<br />
        <br />

        Twilio Client Id: <%=TwilioClientId%><br />
        Twilio Status: <span id="twilioStatus">Offline</span><br />
        <br />

        <div id="callStart">
            <h3>Available Clients</h3>
            <div id="clientList"></div>
            <br />

            <div style="display:none;">
                Number to call: <input type="text" id="tocall" value="">
                <input type="button" id="call" value="Start Call"/>
            </div>
        </div>

        <div id="callIncoming" style="display:none;">
            <h3>Incoming Call</h3>
            <h4 id="callIncomingFrom">Unknown Caller</h4>
            <h4>Handle the Incoming Call</h4>
            <input type="button" value="Answer" id="callIncomingAnswer" /><br />
            <input type="button" value="Answer and Record" id="callIncomingAnswerRecord" /><br />
            <input type="button" value="Voicemail" id="callIncomingVoicemail" /><br />
            <input type="button" value="Ignore" id="callIncomingIgnore" /><br />
            <input type="button" value="Block" id="callIncomingBlock" /><br />
            <br />
            <input type="text" id="callIncomingTransferTo" value="+18007773456" />
            <input type="button" value="Transfer" id="callIncomingTransfer" /><br />
            
        </div>

        <div id="callInProgress" style="display:none;">
            <h3>Call In Progress</h3>
            <h4 id="callInProgressFrom">Unknown Caller</h4>
            <input type="button" id="hangup" value="Hangup Call" />&nbsp&nbsp;
            <input type="button" id="mute" value="Mute" />
            <input type="button" id="unmute" value="Unmute" style="display:none;" /><br />
            <br />
            <div id="callInProgressModify" style="display:none;">
                <h4>Modify the Current Call</h4>
                <input type="button" value="Voicemail" id="callInProgressVoicemail" /><br />
                <input type="button" value="Block" id="callInProgressBlock" /><br />
                <br />
                <input type="text" id="callInProgressTransferTo" value="+18007773456" />
                <input type="button" value="Transfer" id="callInProgressTransfer" /><br />
            </div>
        </div>
        <br />

        <h3>Debugging Variables</h3>
        AspNet Session ID: <%=AspNetSessionId %><br />
        Node Session ID: <span id="nodeSessionId">retrieving...</span><br />
        <br />
    </div>
    </form>

    <script type="text/javascript">

        $(document).ready(function () {

            //*********************************************************
            //socket.io - setup connection and event handlers
            //*********************************************************
            //
            //socketIo events
            //-incoming
            //  -connect
            //  -clientRequest
            //  -serverResponse
            //-outgoing
            //  -clientEvent
            //  -clientResponse
            //  -serverRequest

            var clientRequestData;
            var socket = io.connect("<%=NodeUrl%>");

            socket.on('connect', function () {
                console.log('socket.connect');
                //report back to server the connection has been established
                socket.emit('clientEvent', { EventName: "connect", SessionAspNetId: "<%=AspNetSessionId%>", TwilioClientToken: "<%=TwilioClientToken%>" });
            });
            socket.on('clientRequest', function (data) {
                console.log('socket.clientRequest');
                
                clientRequestData = data;

                //handle incoming call
                if (data && data.EventName == "callincoming" && data.Call) {
                    console.log('socket.clientRequest - callincoming - ' + data.Call.FromName);
                    alert('Incoming call from: ' + data.Call.FromName);
                    toggleCallStatus('incoming', data.Call.FromName);
                } else if (data && data.EventName == "calldisconnect") {
                    console.log('socket.clientRequest - calldisconnect');
                    toggleCallStatus();
                }
            });
            socket.on('serverResponse', function (data) {
                console.log('socket.serverResponse');

                //update the node session value
                $('#nodeSessionId').text(data.SessionNodeId);
            });


            //*********************************************************
            //Twilio - setup connection and event handlers
            //*********************************************************

            var twilioConn = null;
            var callDirection = 'incoming';
            var clientAvailableList = [];

            Twilio.Device.setup("<%=TwilioClientToken%>");
            Twilio.Device.ready(function (device) {
                console.log('twilio.ready');
                $('#twilioStatus').text('Ready to start call');
            });
            Twilio.Device.presence(function (presenceEvent) {
                console.log('twilio.presence');
                
                //setup available clients to call
                var presenceEventInArray = clientAvailableList.getFromArray(presenceEvent, function (e) {
                    return e.from === presenceEvent.from;
                });

                if (presenceEventInArray) {
                    if (presenceEvent.available)
                        presenceEventInArray.available = presenceEvent.available;
                    else
                        clientAvailableList.splice(clientAvailableList.indexOf(presenceEventInArray), 1);
                }
                else if (presenceEvent.available)
                    clientAvailableList.push(presenceEvent);

                //update the available client list
                updateClientList(clientAvailableList, $('#clientList'));
            });
            Twilio.Device.incoming(function (conn) {
                console.log('twilio.incoming');
                
                //accept an incoming call
                $('#twilioStatus').text('Incoming call');
                twilioConn = conn;
                conn.accept();
                callDirection = 'incoming';
            });
            Twilio.Device.connect(function (conn) {
                console.log('twilio.connect');
                
                //call established, in progress
                twilioConn = conn;
                $('#twilioStatus').text("Call in progress");
                toggleCallStatus('inprogress', conn.CallerName, callDirection);
            });
            Twilio.Device.disconnect(function (conn) {
                console.log('twilio.disconnect');
                
                //current call ended

                //try to notify reciever the call has been disconnected
                if (callDirection == 'outgoing' && twilioConn != undefined && twilioConn.message != undefined && twilioConn.message.tocall != undefined) {
                    callDisconnect(socket, twilioConn.message.tocall);
                }

                twilioConn = null;
                $('#twilioStatus').text("Call ended");
                toggleCallStatus();
                callDirection = 'incoming';
            });
            Twilio.Device.offline(function (device) {
                console.log('twilio.offline');
                
                //not connected to the twilio servers
                $('#twilioStatus').text('Offline');
            });
            Twilio.Device.error(function (error) {
                console.log('twilio.error - ' + error);
                
                $('#twilioStatus').text(error);
            });


            //*********************************************************
            //user input events
            //*********************************************************

            //
            //start call events
            //
            $('.callInitiate').live('click', function () {
                //click link in available client list
                var clientId = $(this).attr('client');
                if (clientId) {
                    params = { "tocall": 'client:' + clientId };
                    twilioConn = Twilio.Device.connect(params);
                    callDirection = 'outgoing';
                    toggleCallStatus('inprogress', clientId, callDirection);
                }
            });
            $("#call").live('click', function () {
                //manually entered number, 'call' clicked
                var toCall = $('#tocall').val();
                params = { "tocall": toCall };
                twilioConn = Twilio.Device.connect(params);
                callDirection = 'outgoing';
                toggleCallStatus('inprogress', toCall, callDirection);
            });

            //
            //incoming call events
            //
            $('#callIncomingAnswer').live('click', function () {
                callAnswer(socket, clientRequestData);
                callDirection = 'incoming';
            });
            $('#callIncomingAnswerRecord').live('click', function () {
                callAnswer(socket, clientRequestData, true);
                callDirection = 'incoming';
            });
            $('#callIncomingTransfer').live('click', function () {
                var callIncomingTransferTo = $('#callIncomingTransferTo').val();
                callTransfer(socket, clientRequestData, callIncomingTransferTo);
            });
            $('#callIncomingIgnore').live('click', function () {
                callIgnore(socket, clientRequestData);
            });
            $('#callIncomingBlock').live('click', function () {
                callBlock(socket, clientRequestData);
            });
            $('#callIncomingVoicemail').live('click', function () {
                callVoicemail(socket, clientRequestData);
            });

            //
            //in progress call events
            //
            $("#hangup").live('click', function () {
                Twilio.Device.disconnectAll();
            });
            $("#mute").live('click', function () {
                twilioConn.mute();
                toggleMuteStatus();
            });
            $("#unmute").live('click', function () {
                twilioConn.unmute();
                toggleMuteStatus();
            });
            $('#callInProgressTransfer').live('click', function () {
                var callInProgressTransferTo = $('#callInProgressTransferTo').val();
                callTransfer(socket, clientRequestData, callInProgressTransferTo);
            });
            $('#callInProgressBlock').live('click', function () {
                callBlock(socket, clientRequestData);
            });
            $('#callInProgressVoicemail').live('click', function () {
                callVoicemail(socket, clientRequestData);
            });


            //*********************************************************
            //helper functions
            //*********************************************************

            //
            //call helper functions
            //
            function callAnswer(socket, callData, record) {
                callData.Call.CallRecord = record || false;
                socket.emit('clientResponse', { EventName: "incomingcall", EventResponse: "accept", Call: callData.Call });
                toggleCallStatus('answering', clientRequestData.Call.FromName);
            }
            function callTransfer(socket, callData, transferTo) {
                callData.Call.TransferTo = transferTo;
                socket.emit('clientResponse', { EventName: "incomingcall", EventResponse: "transfer", Call: callData.Call });
                toggleCallStatus();
            }
            function callIgnore(socket, callData) {
                socket.emit('clientResponse', { EventName: "incomingcall", EventResponse: "ignore", Call: callData.Call });
                toggleCallStatus();
            }
            function callBlock(socket, callData) {
                socket.emit('clientResponse', { EventName: "incomingcall", EventResponse: "block", Call: callData.Call });
                toggleCallStatus();
            }
            function callVoicemail(socket, callData) {
                socket.emit('clientResponse', { EventName: "incomingcall", EventResponse: "voicemail", Call: callData.Call });
                toggleCallStatus();
            }
            function callDisconnect(socket, callTo) {
                socket.emit('clientResponse', { EventName: "incomingcall", EventResponse: "disconnect", CallTo: callTo });
                toggleCallStatus();
            }

            //
            //ui helper functions
            //
            function updateClientList(clientAvailableList, clientListElement) {
                var clientListHtml = "";
                for (var i = 0; i < clientAvailableList.length; i++) {
                    clientListHtml += "<a href=\"#\" class=\"callInitiate\" client=\"" + clientAvailableList[i].from + "\">Call " + clientAvailableList[i].from + "</a><br />";
                }
                clientListElement.html(clientListHtml);
            }
            function toggleMuteStatus(reset) {
                if (reset) {
                    $('#mute').show();
                    $('#unmute').hide();
                } else {
                    $('#mute').toggle();
                    $('#unmute').toggle();
                }
            }
            function toggleCallStatus(state, callFrom, callDirection) {
                $('#callIncoming').hide();
                $('#callStart').hide();
                $('#callInProgress').hide();
                $('#callInProgressModify').hide();

                switch (state) {
                    case 'incoming':
                        $('#callIncomingFrom').text(callFrom);
                        $('#callIncoming').show();
                        break;
                    case 'inprogress':
                        $('#callInProgressFrom').text(callFrom);
                        $('#callInProgress').show();
                        toggleMuteStatus(true);
                        if (callDirection == 'incoming') {
                            $('#callInProgressModify').show();
                        }
                        break;
                    case 'answering':
                        $('#twilioStatus').text('Accepting call from ' + callFrom);
                        break;
                    default:
                        $('#callStart').show();
                        break;
                }
            }

            //
            //other helper functions
            //
            Array.prototype.getFromArray = function (element, comparer) {
                for (var i = 0; i < this.length; i++) {
                    if (comparer(this[i]))
                        return this[i];
                }
                return;
            }

        });

    </script>

</body>
</html>
