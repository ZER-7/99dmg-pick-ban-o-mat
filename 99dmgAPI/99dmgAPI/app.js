'use strict';

var mymodule = require('./csgo99damage')

console.log('------------');
mymodule.getOpponents('http://csgo.99damage.de/de/leagues/99dmg/1360-saison-12/group/3325-division-4-66', 'Neinzge Tactics').then(function (out) {
    console.log(out);
});;

console.log('------------');

