using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using PickBan_o_mat.basic;

namespace PickBan_o_mat
{
    internal class DataModel
    {
        private static readonly List<Map> CurrentMapPool = new List<Map>
        {
            Map.Vertigo,
            Map.Inferno,
            Map.Overpass,
            Map.Train,
            Map.Nuke,
            Map.Mirage,
            Map.Dust2
        };

        public Dictionary<Map, int> Get(DecisionType thisType, List<List<Decision>> asT1 = null,
            List<List<Decision>> asT2 = null)
        {
            List<List<Decision>> temp1 = asT1 ?? _pickBanBatchT1;

            List<List<Decision>> temp2 = asT2 ?? _pickBanBatchT2;

            List<Map> mapPool = new List<Map>
            {
                Map.Vertigo,
                Map.Inferno,
                Map.Overpass,
                Map.Train,
                Map.Nuke,
                Map.Mirage,
                Map.Dust2
            };

            int counter = 0;
            Dictionary<Map, int> banned = new Dictionary<Map, int>();

            foreach (Map item in mapPool)
            {
                foreach (List<Decision> singlePickBan in temp1)
                {
                    if (singlePickBan
                            .FirstOrDefault(s => s.Map == item && s.Type == thisType && s.GoingFirst) != null)
                    {
                        counter++;
                    }
                }

                foreach (List<Decision> singlePickBan in temp2)
                {
                    if (singlePickBan
                            .FirstOrDefault(s => s.Map == item && s.Type == thisType && s.GoingFirst == false) != null)
                    {
                        counter++;
                    }
                }

                banned.Add(item, counter);
                counter = 0;
            }

            return banned;
        }

        internal List<string> SimulatePickBan(bool ist1, List<KeyValuePair<Map, int>> ourPool, bool clever = true,
            Strategy withThisStrategy = Strategy.Default)
        {
            List<string> result;
            List<Map> pool = new List<Map>(ourPool.Select(s => s.Key));
            List<KeyValuePair<Map, int>> nmePool = CreateHierarchy();

#if DEBUG
            Debug.WriteLine("Our pool :");
            foreach (KeyValuePair<Map, int> item in ourPool)
            {
                Debug.WriteLine($"{item.Key} | {item.Value}");
            }

            Debug.WriteLine("-----------");

            Debug.WriteLine("Their pool :");

            foreach (KeyValuePair<Map, int> item in nmePool)
            {
                Debug.WriteLine($"{item.Key} | {item.Value}");
            }
#endif

            switch (withThisStrategy)
            {
                case Strategy.Default:
                    result = DoDefaultPickBan(ourPool, nmePool);
                    break;
                case Strategy.Gamble:
                    result = DoGamblePickBan(ourPool, nmePool);
                    break;
                case Strategy.Safe:
                    result = DoSafePickBan(ourPool, nmePool);
                    break;
                case Strategy.RelativeStrength:
                    result = DoRelativeStrengthPickBan(ourPool, nmePool);
                    break;
                default:
                    result = DoDefaultPickBan(ourPool, nmePool);
                    break;
            }

            return result;
        }

        private static List<string> DoRelativeStrengthPickBan(IReadOnlyCollection<KeyValuePair<Map, int>> ourPool,
            IReadOnlyCollection<KeyValuePair<Map, int>> nmePool, bool starting = true)
        {
            List<KeyValuePair<Map, int>> deltas = new List<KeyValuePair<Map, int>>();

            foreach (Map map in ourPool.Select(s => s.Key).ToList())
            {
                deltas.Add(
                    new KeyValuePair<Map, int>(
                        map,
                        ourPool.First(l => l.Key == map).Value -
                        nmePool.First(l => l.Key == map).Value));
            }

            Debug.WriteLine("Relative strenght :");
            foreach (KeyValuePair<Map, int> item in deltas.OrderBy(s => s.Value))
            {
                Debug.WriteLine($"{item.Key} | {item.Value}");
            }

            List<string> result = new List<string>();
            deltas = deltas.OrderBy(s => s.Value).ToList();

            //nme removes the top 2
            // we remove bot 2

            if (starting)
            {
                result.Add(deltas[0].Key.ToString());

                result.Add(deltas[6].Key.ToString());
                result.Add(deltas[5].Key.ToString());

                result.Add(deltas[1].Key.ToString());
            }
            else
            {
                result.Add(deltas[6].Key.ToString());

                result.Add(deltas[0].Key.ToString());
                result.Add(deltas[1].Key.ToString());

                result.Add(deltas[5].Key.ToString());
            }

            result.Add(deltas[2].Key.ToString());
            result.Add(deltas[4].Key.ToString());

            return result;
        }

        private static List<string> DoSafePickBan(List<KeyValuePair<Map, int>> ourPool, List<KeyValuePair<Map, int>> nmePool,
            bool starting = true)
        {
            throw new NotImplementedException();
        }

        private static List<string> DoGamblePickBan(List<KeyValuePair<Map, int>> ourPool, List<KeyValuePair<Map, int>> nmePool,
            bool starting = true)
        {
            throw new NotImplementedException();
        }

        private static List<string> DoDefaultPickBan(IEnumerable<KeyValuePair<Map, int>> ourPool,
            List<KeyValuePair<Map, int>> nmePool, bool starting = true)
        {
            List<string> result = new List<string>();
            List<Map> pool = new List<Map>(ourPool.Select(s => s.Key));

            if (true)
            {
                pool = DoCleverReordering(pool, nmePool);
            }

            Debug.WriteLine("--------------------------------------");
            Debug.WriteLine("-------------SIMULATE--START----------");
            Debug.WriteLine("--------------------------------------");

            if (starting)
            {
                result.Add($"{pool.First()}");
                Debug.WriteLine($"T1 banned {pool.First().ToString()}");

                // remove ban from pools
                nmePool.RemoveAll(s => s.Key == pool.First());
                pool.RemoveAt(0);

                result.Add($"{nmePool.First().Key}");
                result.Add($"{nmePool[1].Key}");
                Debug.WriteLine($"T2 banned {nmePool.First().Key.ToString()}");
                Debug.WriteLine($"T2 banned {nmePool[2].Key.ToString()}");

                // remove their bans
                pool.RemoveAll(s => (s == nmePool[0].Key) | (s == nmePool[1].Key));
                nmePool.RemoveAt(0);
                nmePool.RemoveAt(0);

                result.Add($"{pool.First()}");
                Debug.WriteLine($"T1 banned {pool.First().ToString()}");

                //remove our ban
                nmePool.RemoveAll(s => s.Key == pool.First());
                pool.RemoveAt(0);

                Debug.WriteLine("Current pool : ");
                foreach (Map item in pool)
                {
                    Debug.WriteLine($"{item.ToString()}");
                }

                result.Add($"{nmePool.Last().Key}");
                Debug.WriteLine($"T2 picked {nmePool.Last().Key.ToString()}");
                pool.RemoveAll(s => s == nmePool.Last().Key);
                result.Add($"{pool.Last()}");
                Debug.WriteLine($"T1 picked {pool.Last().ToString()}");
            }
            else
            {
                result.Add($"{nmePool.First().Key}");
                Debug.WriteLine($"T1 banned {nmePool.First().Key.ToString()}");


                // remove ban from pools
                pool.RemoveAll(s => (s == nmePool[0].Key) | (s == nmePool[1].Key));
                nmePool.RemoveAt(0);

                result.Add($"{pool.First()}");
                result.Add($"{pool[1]}");
                Debug.WriteLine($"T2 banned {pool.First().ToString()}");
                Debug.WriteLine($"T2 banned {pool[2].ToString()}");


                // remove ban from pools
                nmePool.RemoveAll(s => (s.Key == pool[0]) | (s.Key == pool[1]));
                pool.RemoveAt(0);
                pool.RemoveAt(0);

                result.Add($"{nmePool.First().Key}");
                Debug.WriteLine($"T1 banned {nmePool.First().Key.ToString()}");

                // remove ban from pools
                nmePool.RemoveAt(0);
                pool.RemoveAll(s => s == nmePool.First().Key);

                Debug.WriteLine("Current pool : ");

                result.Add($"{pool.Last()}");
                Debug.WriteLine($"T2 picked {pool.Last().ToString()}");
                nmePool.RemoveAll(s => s.Key == pool.Last());
                result.Add($"{nmePool.Last().Key}");
                Debug.WriteLine($"T1 picked {nmePool.Last().Key.ToString()}");
            }


            Debug.WriteLine("--------------------------------------");
            Debug.WriteLine("-------------SIMULATE--END------------");
            Debug.WriteLine("--------------------------------------");

            return result;
        }

        public List<KeyValuePair<Map, int>> CreateHierarchy(List<List<Decision>> asT1 = null,
            List<List<Decision>> asT2 = null)
        {
            const int modBan = -2;
            const int modPick = 2;
            const int modPool = 1;

            List<List<Decision>> temp1 = asT1 ?? _pickBanBatchT1;

            List<List<Decision>> temp2 = asT2 ?? _pickBanBatchT2;

            List<KeyValuePair<Map, int>> nmePool = GetMapHierachy(asT1, asT2).OrderBy(x => x.Value).ToList();
            List<KeyValuePair<Map, int>> nmePicks = Get(DecisionType.Pick, asT1, asT2).OrderBy(x => x.Value).ToList();
            List<KeyValuePair<Map, int>> nmeBans = Get(DecisionType.Ban, asT1, asT2).OrderBy(x => x.Value).ToList();

            Debug.WriteLine($" Before reordering {nmePool.Select(s => s.Key.ToString())}");

            List<KeyValuePair<Map, int>> result = nmePool
                .Select(s => s.Key).
                Select(map => new KeyValuePair<Map, int>(map, modBan * nmeBans.First(s => s.Key == map).Value + modPool * nmePool.First(s => s.Key == map).Value + modPick * nmePicks.First(s => s.Key == map).Value)).ToList();

            Debug.WriteLine($" After reordering {result.Select(s => s.Key.ToString())}");

            return result.OrderBy(s => s.Value).ToList();
        }

        private static List<Map> DoCleverReordering(List<Map> ourPool, IReadOnlyCollection<KeyValuePair<Map, int>> nmePool)
        {
            List<Map> oldPool = new List<Map>();
            foreach (Map item in ourPool)
            {
                oldPool.Add(item);
            }

            foreach (Map item in oldPool.Take(3))
            {
                if (!nmePool.Select(s => s.Key).Take(3).Contains(item))
                {
                    continue;
                }

                // push down 3
                ourPool.Remove(item);
                ourPool.Insert(2, item);
            }

            return ourPool;
        }


        public Dictionary<Map, int> GetMapHierachy(List<List<Decision>> asT1 = null, List<List<Decision>> asT2 = null)
        {
            List<List<Decision>> temp1 = asT1 ?? _pickBanBatchT1;

            List<List<Decision>> temp2 = asT2 ?? _pickBanBatchT2;

            int counter = 0;
            Dictionary<Map, int> banned = new Dictionary<Map, int>();

            foreach (Map item in CurrentMapPool)
            {
                foreach (List<Decision> singlePickBan in temp1)
                {
                    Decision dec1 = singlePickBan
                        .FirstOrDefault(s => s.Map == item && s.Type != DecisionType.Ban);
                    if (dec1 != null)
                    {
                        counter++;
                    }
                }

                foreach (List<Decision> singlePickBan in temp2)
                {
                    Decision dec2 = singlePickBan
                        .FirstOrDefault(s => s.Map == item && s.Type != DecisionType.Ban);
                    if (dec2 != null)
                    {
                        counter++;
                    }
                }

                banned.Add(item, counter);
                counter = 0;
            }

            banned.OrderByDescending(key => key.Value);

            return banned;
        }

        private static DecisionType GetDecision(string action)
        {
            return action.Contains("bans") ? DecisionType.Ban : DecisionType.Pick;
        }

        internal static List<List<Decision>> BatchPickBan(string t1Text, bool isTeam1)
        {
            List<List<Decision>> pickBanBatch = new List<List<Decision>>();

            using (StringReader ms = new StringReader(t1Text))
            {
                string curLine;
                while ((curLine = ms.ReadLine()) != "")
                {
                    if (!Equals(curLine, null))
                    {
                        pickBanBatch.Add(PickBan(isTeam1, curLine));
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return pickBanBatch;
        }

        private static Map GetMap(string action)
        {
            string temp = action.Split(' ').Last();
            if (temp.Contains(','))
            {
                temp = temp.Remove(temp.IndexOf(",", StringComparison.Ordinal));
            }

            switch (temp)
            {
                case "de_inferno":
                    return Map.Inferno;
                case "de_overpass":
                    return Map.Overpass;
                case "de_nuke":
                    return Map.Nuke;
                case "de_train":
                    return Map.Train;
                case "de_vertigo":
                    return Map.Vertigo;
                case "de_mirage":
                    return Map.Mirage;
                case "de_dust2":
                    return Map.Dust2;
                default:
                    return Map.Unknown;
            }
        }

        public List<Map> GetPool(List<string> sPool)
        {
            // remove already handled map
            sPool.RemoveAt(0);
            List<Map> tempPool = new List<Map>();
            foreach (string action in sPool)
            {
                tempPool.Add(GetMap(action));
            }

            return tempPool;
        }

        private static bool GetTeamNumber(string action)
        {
            return action.Contains("T1");
        }

        private static List<Decision> PickBan(bool isTeam1, string pbString)
        {
            List<Decision> pickBan = new List<Decision>();
            List<Map> banned = new List<Map>();

            List<string> pickBanList = pbString.Split(',').ToList();
            for (int i = pickBanList.Count; i > 0; i++)
            {
                string action = pickBanList.FirstOrDefault();
                if (Equals(action, null))
                {
                    break;
                }

                // Add to the banned pool
                banned.Add(GetMap(action));

                Decision curDec = new Decision
                {
                    Type = GetDecision(action),
                    Map = GetMap(action),
                    Pool = new List<Map>(CurrentMapPool),
                    GoingFirst = GetTeamNumber(action)
                };

                curDec.Pool.RemoveAll(s => banned.Contains(s));

                pickBan.Add(curDec);

                pickBanList.RemoveAt(0);
            }

            return pickBan;
        }

        internal void SetPickBanData(string t1Text, string t2Text)
        {
            _pickBanBatchT1 = BatchPickBan(t1Text, true);
            _pickBanBatchT2 = BatchPickBan(t2Text, false);
        }

        #region Member

        private List<List<Decision>> _pickBanBatchT1;

        private List<List<Decision>> _pickBanBatchT2;

        #endregion
    }
}