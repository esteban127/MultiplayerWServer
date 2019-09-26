var shortid = require('shortid')

module.exports = class Player{
    constructor(){
        this.name = "user_default";
        this.id = shortid.generate();
    }
}