// SocketIO4Net - nodejs SocketIO Server
// version: v0.5.17
// node.exe v0.6.7

// require command-line options to start this application
var argv = require('optimist')
    .usage('Usage: $0 -host [localhost] -port [3000]')
    .demand(['host', 'port'])
    .argv;

var express = require('express')
  , server = express.createServer()
  , socketio = require('socket.io')

  // configure Express
server.configure(function () {
    server.use(express.bodyParser());
    server.use('/content', express.static(__dirname + '/content'));
    server.use('/scripts', express.static(__dirname + '/scripts'));
    });

// start server listening at host:port
server.listen(argv.port, argv.host); // http listen on host:port e.g. http://localhost:3000/

// configure Socket.IO
var io = socketio.listen(server); // start socket.io
io.set('log level', 1);


console.log('');
console.log('Nodejs Version: ',process.version);
console.log('     Listening: http://',argv.host, ':', argv.port);
console.log('');

// ***************************************************************
//    WEB Handlers  
//    Express guid: http://expressjs.com/guide.html
// ***************************************************************
server.get('/', function (req, res) {
    res.sendfile(__dirname + '/appEventsClient.html');
});

// ***************************************************************
//    Socket.IO Client Handlers
// ***************************************************************
io.sockets.on('connection', function (socket) {

    //io.sockets.emit('newConnection'); // broadcast to all clients
    //socket.emit('clientEvent', { Event: 'connected to nodeJs', ClientId: socket.id });  // only the current connected socket will receive this event
    socket.emit('serverResponse', { EventName: 'connected', SessionNodeId: socket.id });  // only the current connected socket will receive this event

//    socket.on('clientResponse', function (data) {
//        console.log('On clientResponse: ', data);
//    });
//
//    socket.on('serverRequest', function (data) {
//        console.log('On serverRequest: ' , data);
//    });
//
//    socket.on('messageAck', function (data, fn) {
//        console.log('messageAck: ' + data);
//        if (fn != 'undefined') {
//            console.log(fn);
//            fn('woot');
//        }
//    });


	//
	//incoming messages
	//
	socket.on("clientEvent", function (data) {
		io.sockets.emit('clientEventInternal', { SessionNodeId: socket.id, ClientData: data });
	});
	
	socket.on("clientResponse", function (data) {
		io.sockets.emit('clientResponseInternal', { SessionNodeId: socket.id, ClientData: data });
	});

	socket.on("serverRequest", function (data) {
		io.sockets.emit('serverRequestInternal', { SessionNodeId: socket.id, ClientData: data });
	});

	socket.on('disconnect', function () {
        //io.sockets.emit('clientEvent', { event: 'disconnected' }); // broadcast event to all clients (no data)
        io.sockets.emit('clientEventInternal', { SessionNodeId: socket.id, ClientData: { EventName: 'disconnect' }});
    });
    
	//
	//outogoing messages
	//
	socket.on("clientRequestInternal", function (data) {
		console.log(new Date().getTime() + '-On clientRequestInternal: ' + data.SessionNodeId);
		io.sockets.socket(data.SessionNodeId).emit('clientRequest', data);
		console.log('On clientRequestInternal - sent to: ' + data.SessionNodeId);
	});
	
	socket.on("serverResponseInternal", function (data) {
		//console.log('On serverResponseInternal: ' + data);
		io.sockets.socket(data.SessionNodeId).emit('serverResponse', data);
		//console.log('On serverResponseInternal - sent to: ' + data.SessionNodeId);
	});
	
    
});





