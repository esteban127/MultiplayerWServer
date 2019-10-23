var ServerObject = require('./ServerObject.js')

module.exports = class Bullet extends ServerObject{    
    constructor(){
        super();
        this.velocity = new vector3();
    }
    onUpdate(){
        position = position + velocity;
    }
}