var io = require('socket.io')(process.env.io || 8080);
var Player = require('./src/Player.js')

console.log('Server Initialized');

var players = [];
var sockets = [];

io.on('connection',function(socket){    
    console.log('Se conectó un roberto');

    var player = new Player();
    players[player.id]= player;
    sockets[player.id]= socket;

    socket.emit('onRegister',{id: player.id});   
    socket.emit('spawn', player);
    socket.broadcast.emit('spawn',player)
    for(var otherPlayer in players){
        if(otherPlayer != player.id){            
            socket.emit('spawn',players[otherPlayer]);
            console.log(otherPlayer);
            console.log(player.id);
        }
    }        

    socket.on('disconnect',function(){
        console.log('bai roberto');  
        delete(players[player.id]);
        delete(sockets[player.id]);
        socket.broadcast.emit('playerDisconnected',player)              
    });
});