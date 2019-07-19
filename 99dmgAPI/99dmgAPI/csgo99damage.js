
function time() {
    return Math.round((new Date()).getTime() / 1000);
}

function timeByDate(date, hours) {
    var date = date.split(".");
    return new Date(date[1] + "/" + date[0] + "/" + date[2] + ' ' + hours + ':00').getTime() / 1000;
}

function getSeasonpages(allseasons) {
    var seasonpages = [];
    for (var index in allseasons) {
        if (allseasons.length - 2 <= index) {
            seasonpages.push(allseasons[index]['link']);
        }
    }
    return seasonpages;
}

function getVote(html) {
    var _ = require("underscore");
    var cheerio = require('cheerio');
    var $ = cheerio.load(html);

    const result = $("#match_log tr").map((i, element) => ({
        Zeit: $(element).find('td:nth-of-type(1)').text().trim(),
        Benutzer: $(element).find('td:nth-of-type(2)').text().trim(),
        Aktion: $(element).find('td:nth-of-type(3)').text().trim(),
        Details: $(element).find('td:nth-of-type(4)').text().trim()
    })).get()


    var filtered = _.where(result, { Aktion: "mapvote_ended" });
    //console.log("============================");
    //console.log(filtered);
    //console.log("----------------------------");
    var first = filtered[0];
    //console.log(first);
    //console.log(first.hasOwnProperty('Details'));
    var output = [];

    if (first && first.hasOwnProperty('Details') && first.hasOwnProperty('Aktion')) {
        if (first['Details'] != 'timeouted') {
            output.push(first['Details']);
        } else {
            console.log('timeouted.')
        }
    }

    return output;
}

///
/// exported functions
///

// gets all seasonpages a team played ever give a team id
function getAllSeasons(teamID) {
    var cheerio = require('cheerio'),
        request = require('request');
    return new Promise(function (resolve, reject) {
        request({
            url: 'https://csgo.99damage.de/de/leagues/teams/' + teamID,
            headers: { 'User-Agent': 'Mozilla/5.0 (Windows NT 6.1; WOW64; rv:46.0) Gecko/20100101 Firefox/46.0' }
        }, function (err, res, html) {
            if (!err) {
                var $ = cheerio.load(html);
                const result = $("#content h2:contains(Werdegang)+table tr").map((i, element) => ({
                    Season: $(element).find('td:nth-of-type(1)').text().trim(),
                    link: $(element).find('td:nth-of-type(2)').children().attr('href'),
                    Score: $(element).find('td:nth-of-type(3)').text().trim()
                })).get()

                resolve(result);
            } else {
                reject(err);
            }
        });
    })
}

// gets a list of all matchIDs from the seasonpage
function getMatchesThisSeason(page, team) {
    var cheerio = require('cheerio'),
        request = require('request');
    return new Promise(function (resolve, reject) {
        request({
            url: page,
            headers: { 'User-Agent': 'Mozilla/5.0 (Windows NT 6.1; WOW64; rv:46.0) Gecko/20100101 Firefox/46.0' }
        }, function (err, res, html) {
            if (!err) {
                var _ = require("underscore");

                var $ = cheerio.load(html);

                const result = $(".league_table_matches tr").map((i, element) => ({
                    Date: $(element).find('td:nth-of-type(1)').text().trim(),
                    Team1: $(element).find('td:nth-of-type(2)').text().trim(),
                    Team2: $(element).find('td:nth-of-type(3)').text().trim().replace('vs.  ', ''),
                    score: $(element).find('td:nth-of-type(4)').text().trim(),
                    link: $(element).find('td:nth-of-type(2)').children().attr('href')
                })).get()

                var asT1 = _.where(result, { Team1: team });
                var asT2 = _.where(result, { Team2: team });
                var filtered = asT1.concat(asT2);

                //add all links from matches that have a score
                var trimmed = [];
                // match was finished
                var regex = '(0|1|2):(0|1|2)'
                for (var index in filtered) {
                    if (filtered[index]['score'].match(regex)) {
                        var temp = filtered[index]['link'];
                        trimmed.push(temp.substr(temp.lastIndexOf('/') + 1));
                    }
                }

                resolve(trimmed);

            } else {
                reject(err);
            }
        });
    })
}

// all pick bans for the last two seasons
async function getPickBan(teamID, callback) {
    var cheerio = require('cheerio'),
        request = require('request');
    var _ = require("underscore");
    //get all season pages
    var pages = await csgo99damage.getAllSeasons(teamID);
    pages = getSeasonpages(pages);
    console.log(pages);

    var shortName = await csgo99damage.getShorthand(teamID);
    var longName = await csgo99damage.getLongName(teamID);

    //var tst = await csgo99damage.getOpponents(pages[pages.length - 1], longName);
    //console.log(tst);

    var matches = [];
    for (var index in pages) {
        var temp = await csgo99damage.getMatchesThisSeason(pages[index], shortName);
        matches.push(temp);
    }

    matches = _.flatten(matches);
    var matchdata = [];

    //get all the matchdata
    for (var i in matches) {
        var data = await getMatch(matches[i])
        matchdata.push(data);
    }

    var res = {};
    var t1 = [];
    var t2 = [];
    // remove all invalid matches
    for (var i in matchdata) {
        var element = matchdata[i];
        // check if valid
        if (element && element.hasOwnProperty('pickban') && element.hasOwnProperty('team1') && element.hasOwnProperty('team2') && element.hasOwnProperty('status')) {
            // check if match has correct status. This eliminates DefWins
            // status 2 is played and done.
            if (element['status'] == 2) {
                if (element['team1'] == longName) {
                    t1.push(element['pickban']);
                } else if (element['team2'] == longName) {
                    t2.push(element['pickban']);
                }
            }
        }
    }
    // filter out empty arrays
    t1 = t1.filter(e => e.length);
    t2 = t2.filter(e => e.length);

    res['T1'] = t1;
    res['T2'] = t2;

    console.log(res);

    return res;
}

// prints the truth for testing
function printtest() {
    console.log("Node.js is evil!");
}

// get the whole team name of a league team give the correct team id
async function getLongName(teamID) {
    var cheerio = require('cheerio'),
        request = require('request');
    return new Promise(function (resolve, reject) {
        request({
            url: 'https://csgo.99damage.de/de/leagues/teams/' + teamID,
            headers: { 'User-Agent': 'Mozilla/5.0 (Windows NT 6.1; WOW64; rv:46.0) Gecko/20100101 Firefox/46.0' }
        }, function (err, res, html) {
            if (!err) {
                var $ = cheerio.load(html);
                const result = $("#content div h2").text()
                var out = result.substr(0, result.indexOf('(')).trim();
                console.log(out);
                resolve(out);
            } else {
                reject(err);
            }
        });
    });
}

// get the shorthand of a league team give the correct team id
async function getShorthand(teamID) {
    var cheerio = require('cheerio'),
        request = require('request');
    return new Promise(function (resolve, reject) {
        request({
            url: 'https://csgo.99damage.de/de/leagues/teams/' + teamID,
            headers: { 'User-Agent': 'Mozilla/5.0 (Windows NT 6.1; WOW64; rv:46.0) Gecko/20100101 Firefox/46.0' }
        }, function (err, res, html) {
            if (!err) {
                var $ = cheerio.load(html);
                const result = $("#content div h2").text()

                var regExp = /\(([^)]+)\)/;
                var matches = regExp.exec(result);

                console.log(matches[1]);

                resolve(matches[1]);
            } else {
                reject(err);
            }
        });
    });
}

// gets all available data for a give match id
function getMatch(matchID) {
    var cheerio = require('cheerio'),
        request = require('request');

    return new Promise(function (resolve, reject) {
        request({
            url: 'http://csgo.99damage.de/de/leagues/matches/' + matchID,
            headers: { 'User-Agent': 'Mozilla/5.0 (Windows NT 6.1; WOW64; rv:46.0) Gecko/20100101 Firefox/46.0' }
        }, function (err, res, html) {
            if (!err) {
                var $ = cheerio.load(html);
                $('#content').each(function () {
                    var winner = 0, status = 0,
                        score = $(this).find('.score').text().trim(),
                        date = $(this).find('.right').text().replace(' CEST', '').split(', '),
                        team1logo = $($(this).find('.team_logo img')[0]).attr('src'),
                        team2logo = $($(this).find('.team_logo img')[1]).attr('src')
                    streams = [],
                        pickban = getVote(html);
                    $('#content div').each(function (k) {
                        if (k == 42) {
                            var as = $(this).find('a');
                            for (var i = 0; i < as.length; i++) {
                                streams.push([$(as[i]).text(), $(as[i]).attr('href')]);
                            }
                        }
                    });
                    if (score.indexOf('verschoben') >= 0) {
                        winner = 3;
                        status = 4;
                    } else if (score.indexOf('LIVE!') >= 0) {
                        status = 1;
                    } else {
                        if (score.indexOf('defwin') >= 0)
                            status = 3
                        else
                            status = 2;
                        var split = score.substr(0, 5).split(':');
                        split[0] = parseInt(split[0]);
                        split[1] = parseInt(split[1]);
                        if (split[0] < split[1])
                            winner = 2;
                        else if (split[0] == split[1])
                            winner = 3;
                        else
                            winner = 1;
                    }
                    csgo99damage.temp.matches[matchID] = {
                        team1: $($(this).find('.team')[0]).text().trim(),
                        team2: $($(this).find('.team')[1]).text().trim(),
                        team1logo: team1logo,
                        team2logo: team2logo,
                        matchID: matchID,
                        winner: winner,
                        status: status,
                        streams: streams,
                        pickban: pickban,
                        start: timeByDate(date[0], date[1])
                    }
                    resolve(csgo99damage.temp.matches[matchID]);
                });
            } else {
                reject(err);
            }
        });
    });
};

// gets all matches currently playing
function getMatches(callback) {
    var cheerio = require('cheerio'),
        request = require('request');
    if (this.temp.lastUpdate >= 1 && time() - this.temp.lastUpdate < 300) {
        callback(null, this.temp.matches)
    } else {
        request({
            url: 'http://csgo.99damage.de/de/leagues/matches/',
            headers: { 'User-Agent': 'Mozilla/5.0 (Windows NT 6.1; WOW64; rv:46.0) Gecko/20100101 Firefox/46.0' }
        }, function (err, res, html) {
            if (!err) {
                csgo99damage.temp.matches = {};
                csgo99damage.temp.lastUpdate = time();
                var $ = cheerio.load(html);
                $('#content > .item').each(function () {

                    var matchID = $($(this).find('a')[1]).attr('href').substr(35),
                        span = $(this).find('span'),
                        status = $(span[3]).text().trim().toLowerCase(),
                        winner = 0,
                        team1 = $(span[4]).text().trim(),
                        team2 = $(span[2]).text().trim(),
                        state = status,
                        hour = '00:00';

                    matchID = matchID.substr(0, matchID.indexOf('-'));

                    if (status == 'defwin') status = 3;
                    else if (status == 'live') status = 1;
                    else if (status == 'versch.') status = 4;
                    else if (status.indexOf('h') < 0) status = 2;
                    else status = 0;

                    if (status == 0) {
                        hour = state.substr(0, 5);
                    }
                    if (status == 2 && state.indexOf(':') >= 0) {
                        var win = state.split(':');
                        win[0] = parseInt(win[0]);
                        win[1] = parseInt(win[1]);
                        if (win[0] == win[1]) {
                            winner = 3;
                        } else if (win[0] < win[1]) {
                            winner = 1;
                        } else {
                            winner = 2;
                        }
                    }

                    if (team1 != team2) csgo99damage.temp.matches[matchID] = {
                        status: status,
                        winner: winner,
                        start: timeByDate($(span[1]).text().trim(), hour),
                        team1: team1,
                        team2: team2
                    }
                });
                callback(null, csgo99damage.temp.matches);
            } else {
                callback(err);
            }
        });
    }
};

// gets a list of all oppenents from the league page
// excludes the give team
async function getOpponents(page, team) {
    var cheerio = require('cheerio'),
        request = require('request');
    return new Promise(function (resolve, reject) {
        request({
            url: page,
            headers: { 'User-Agent': 'Mozilla/5.0 (Windows NT 6.1; WOW64; rv:46.0) Gecko/20100101 Firefox/46.0' }
        }, function (err, res, html) {
            if (!err) {
                var $ = cheerio.load(html);

                const result = $(".league_table tr").map((i, element) => ({
                    Name: $(element).find('td:nth-of-type(2)').text().trim(),
                    link: $(element).find('td:nth-of-type(2) a').attr('href')
                })).get()

                result.shift();
                result.shift();
                console.log(result);

                //add all links from matches that have a score
                var trimmed = [];
                // match was finished
                for (var index in result) {
                    if (result && result[index].hasOwnProperty('link') && result[index].hasOwnProperty('Name')) {
                        if (result[index]['Name'] != team) {

                            var temp = result[index]['link'];
                            console.log(temp);
                            temp = temp.substr(temp.lastIndexOf('/') + 1);
                            temp = temp.substr(0, temp.indexOf('-'));
                            console.log(temp);
                            trimmed.push(temp);
                        }

                    }
                }

                console.log(trimmed);

                resolve(trimmed);

            } else {
                reject(err);
            }
        });
    })
}


var csgo99damage = {
    temp: {
        lastUpdate: 0,
        matches: {}
    },
    getAllSeasons: getAllSeasons,
    getShorthand: getShorthand,
    getMatchesThisSeason: getMatchesThisSeason,
    getLongName: getLongName,
    getOpponents: getOpponents,
    printtest: printtest,
    getPickBan: getPickBan,
    getMatches: getMatches,
    getMatch: getMatch
}

module.exports = csgo99damage;
