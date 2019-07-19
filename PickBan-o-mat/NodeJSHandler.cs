using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using EdgeJs;

namespace PickBan_o_mat
{
    internal static class NodeJsHandler
    {
        private static async Task<ExpandoObject> GetMatch(int teamId)
        {
            Func<object, Task<object>> getMatch = Edge.Func(@"
var mymodule = require('99dmgapi');

return function (data, callback) {
    mymodule.getMatch(data).then(function(out) {
    callback(null, out)
    });
}
");

            object _short = await getMatch(teamId);

            return _short as ExpandoObject;
        }

        internal static async Task<string> GetTeamName(int teamId)
        {
            Func<object, Task<object>> getName = Edge.Func(@"
var mymodule = require('99dmgapi');

return function (data, callback) {
    mymodule.getLongName(data).then(function(out) {
    callback(null, out)
    });
}
");

            object name = await getName(teamId);

            return name.ToString();
        }

        internal static async Task<string> GetShortHand(int teamId)
        {
            Func<object, Task<object>> getShort = Edge.Func(@"
var mymodule = require('99dmgapi');

return function (data, callback) {
    mymodule.getShorthand(data).then(function(out) {
    callback(null, out)
    });
}
");

            object _short = await getShort(teamId);

            return _short.ToString();
        }

        internal static async Task<List<int>> GetOpponents(int teamId)
        {
            Func<object, Task<object>> getShort = Edge.Func(@"
    var mymodule = require('99dmgapi')

        return function(data, callback)
        {
            mymodule.getOpponentsCurrentSeason(data).then(function(out) {
                callback(null, out)
            });
        }
");

            object _short = await getShort(teamId);

            return (from item in (object[]) _short select Convert.ToInt32(item)).ToList();
        }

        internal static async Task<Dictionary<string, Tuple<int, bool>>> GetAreWeStarting(int teamId)
        {
            Func<object, Task<object>> getMatches = Edge.Func(@"
var mymodule = require('99dmgapi');

return function (data, callback) {
    mymodule.allThisSeasonMatches(data).then(function(out) {
    callback(null, out)
    });
}
");

            object _short = await getMatches(teamId);
            List<int> ret = (from item in (object[]) _short select Convert.ToInt32(item)).ToList();

            string name = await GetTeamName(teamId);

            // matchid, 
            Dictionary<string, Tuple<int, bool>> team1Table = new Dictionary<string, Tuple<int, bool>>();

            foreach (int item in ret)
            {
                ExpandoObject temp = await GetMatch(item);
                string t1 = ((IDictionary<string, object>) temp)["team1"].ToString();
                string t2 = ((IDictionary<string, object>) temp)["team2"].ToString();
                string team = t1 == name ? t2 : t1;

                team1Table.Add(team, new Tuple<int, bool>(item, t1 == name));
            }

            return team1Table;
        }

        internal static async Task<List<List<string>>> GetPickBan(int nextMatch)
        {
            Func<object, Task<object>> getPickBan = Edge.Func(@"
var mymodule = require('99dmgapi');

return function (data, callback) {
    mymodule.getPickBan(data).then(function(out) {
    callback(null, out)
    });
}
");

            object _short = await getPickBan(nextMatch);

            object[] t1 = (_short as IDictionary<string, object>)?["T1"] as object[];
            object[] t2 = (_short as IDictionary<string, object>)?["T2"] as object[];

            return new List<List<string>> { ConvertPickBan(t1), ConvertPickBan(t2) };
        }

        private static List<string> ConvertPickBan(IEnumerable<object> t)
        {
            return (from object[] item in t select item[0].ToString()).ToList();
        }

        internal static async Task<List<int>> GetAllMatches(int teamid)
        {
            Func<object, Task<object>> getMatches = Edge.Func(@"
var mymodule = require('99dmgapi');

return function (data, callback) {
    mymodule.allThisSeasonMatches(data).then(function(out) {
    callback(null, out)
    });
}
");

            object _short = await getMatches(teamid);

            return (from item in (object[]) _short select Convert.ToInt32(item)).ToList();
        }

        internal static async Task<Dictionary<string, string>> GetGameDays(int teamid, string shortHand)
        {
            Func<object, Task<object>> getMatches = Edge.Func(@"
var mymodule = require('99dmgapi');

return function (data, callback) {
    mymodule.getGameDays(data).then(function(out) {
    callback(null, out)
    });
}
");

            object _short = await getMatches(teamid);

            Dictionary<string, string> matchOrder = new Dictionary<string, string>();

            foreach (object item in (object[]) _short)
            {
                string t1 = (item as IDictionary<string, object>)?["Team1"].ToString();
                string t2 = (item as IDictionary<string, object>)?["Team2"].ToString();
                string temp = t1 == shortHand ? t2 : t1;
                string score = (item as IDictionary<string, object>)?["score"].ToString();

                matchOrder.Add(temp ?? throw new InvalidOperationException(), score);
            }

            return matchOrder;
        }
    }
}