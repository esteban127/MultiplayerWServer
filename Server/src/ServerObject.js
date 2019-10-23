var shortid = require('shortid')
var vector3 = require('./Vector3.js')


module.exports = class ServerObject{
    constructor(){
        this.id = shortid.generate();
        this.position = new vector3();
    }
}