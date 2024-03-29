var io = require('socket.io')(process.env.io || 8080);
var Player = require('./src/Player.js')
var Bullet = require('./src/Bullet.js')

console.log('Server Initialized');

var players = [];
var actionData = [];
var sockets = [];
var bullets = [];

update(() => {
    bullets.forEach(element => {
        bullet.onUpdate();
        players.forEach(elemet=>{
            sockets[player.id].emit('updateObj',bullet);
        })
    });
}, 100, 0);

io.on('connection',function(socket){        

    var player = new Player();
    console.log('Se conectó un roberto de id ' + player.id);
    players[player.id]= player;
    sockets[player.id]= socket;

    socket.emit('onRegister',{id: player.id});   
         
    socket.on('joinLobby',function(){
        console.log('Joined');
        socket.emit('playerJoin', player);
        socket.broadcast.emit('playerJoin',player)
        for(var otherPlayer in players){
            if(otherPlayer != player.id){            
                socket.emit('playerJoin',players[otherPlayer]);
                console.log(otherPlayer);
                console.log(player.id);
            }
        }   
    })
    socket.on('readyToEndTurn',function(){        
        player.ready = true;
        if(player.ready){
            console.log("PlayerReady")
        }
        var allReady = true;
        for(var playerID in players){
            if(!players[playerID].ready){
                allReady = false;
            }
        }   
        if(allReady){
            for(var playerID in players){
                players[playerID].ready = false;
            } 
            console.log("EndTurn");
            socket.emit('endTurn');
            socket.broadcast.emit('endTurn');            
        }

    })
    socket.on('cancelReadyToEndTurn',function(){        
        player.ready = false;        

    })
    socket.on('submitActionData',function(data){        
        actionData[player.id] = data;
        console.log("DataSubmited on " + player.id);
        var allSubmited = true;
        for(var playerID in players){
            if(actionData[playerID]== null)
            {
                allSubmited = false; 
                console.log("falto yo wacho");            
            }
            else
            {
                console.log(actionData[playerID]);
            }
        } 
        if(allSubmited){
            for(var playerID in players){               
            socket.emit('recieveActionData',actionData[playerID]); 
            socket.broadcast.emit('recieveActionData',actionData[playerID]); 
            }   
            actionData = [];
            console.log("StartActionTurn");
            socket.emit('startActionTurn'); 
            socket.broadcast.emit('startActionTurn');
        }
    })
    socket.on('replicationEnded',function(){
        //socket.broadcast('sincroniceData',data)
        player.ready = true;
        if(player.ready){
            console.log("PlayerReady")
        }
        var allReady = true;
        for(var playerID in players){
            if(!players[playerID].ready){
                allReady = false;
            }
        }   
        if(allReady){
            for(var playerID in players){
                players[playerID].ready = false;
            } 
            console.log("newTurn");
            socket.emit('newTurn');
            socket.broadcast.emit('newTurn');          
        }
                 
    })
    socket.on('gameHosted',function(data){        
        socket.broadcast.emit('startGame');
    })
    socket.on('update',function(data){        
        player.position.x = data.position.x;
        player.position.y = data.position.y;
        player.position.z = data.position.z; 
        socket.broadcast.emit('updateIn',player);
    })
    socket.on('setName',function(data){
        var message = data.message; 
        player.name = message;
    })
    socket.on('kick',function(data){        
        var message = data.message;   
        sockets[message].emit('kicked');
    })
    socket.on('closeLobby',function(data){
        socket.broadcast.emit('kicked');
        socket.emit('kicked');
    })
    socket.on('disconnect',function(){
        console.log('bai roberto');  
        delete(players[player.id]);
        delete(sockets[player.id]);
        socket.broadcast.emit('playerDisconnected',player)              
    });
    /*socket.on('fireBullet',function(data){
        var bullet = new Bullet();
        bullet.velocity.x = data.velocity.x;        
        bullet.velocity.y = data.velocity.y;        
        bullet.velocity.z = data.velocity.z;
        bullet.position.x = data.position.x;        
        bullet.position.y = data.position.y;
        bullet.position.z = data.position.z;
        bullets[bullet.id] = bullet;
        socket.emit('spawnObject');
        socket.broadcast('spawnObject');
    });*/
});

function update(func, wait, loops) {
    var interv = function (w, l) {
        return function () {
            if (typeof l === "undefined" || l-- > 0) {
                setTimeout(interv, w);
                try {
                    func.call(null);
                }
                catch (e) {
                    l = 0; throw e.toString();
                }
            }
        };
    }(wait, loops);
    
    setTimeout(interv, wait);
}