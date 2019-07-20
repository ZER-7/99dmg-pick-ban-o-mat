using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace PickBan_o_mat
{
    internal class Viewmodel : INotifyPropertyChanged
    {
        private const string Space = "\t";
        private const string TEAMURL_FILE_PATH = "\\" + "RememberTeamURL.txt";

        public Viewmodel()
        {
            _dm = new DataModel();

            Init();
            this.LoadUrl();

            _ourMapPool = new List<KeyValuePair<Map, int>>
            {
                new KeyValuePair<Map, int>(Map.Mirage, 0),
                new KeyValuePair<Map, int>(Map.Dust2, 0),
                new KeyValuePair<Map, int>(Map.Vertigo, 0),
                new KeyValuePair<Map, int>(Map.Nuke, 0),
                new KeyValuePair<Map, int>(Map.Train, 0),
                new KeyValuePair<Map, int>(Map.Overpass, 0),
                new KeyValuePair<Map, int>(Map.Inferno, 0)
            };

            StartPickBanCommand = new DelegateCommand(
                s =>
                {
                    try
                    {
                        IsWorking = true;

                        PrintResultsToConsole();
                        SetPickBanSimulation(_dm.SimulatePickBan(IsT1, _ourMapPool));

                        IsWorking = false;
                    }
                    catch (Exception)
                    {
                        MessageQueue.Enqueue("Please Enter valid data", true);
                    }
                }
            );

        }

        private void LoadUrl()
        {
            string file = Directory.GetCurrentDirectory() + TEAMURL_FILE_PATH;
            if (File.Exists(file))
            {
                using (StreamReader reader = new StreamReader(file))
                {
                    TeamUrl = reader.ReadLine();
                    ValidUrl = true;
                }
            }
        }

        private void SaveUrl()
        {
            try
            {
                string file = Directory.GetCurrentDirectory() + TEAMURL_FILE_PATH;
                using (StreamWriter writer = new StreamWriter(file))
                {
                    writer.Write(TeamUrl);
                }
            }
            catch (IOException)
            {

            }
        }

        private void Init()
        {
            IsT1 = true;
            IsWorking = true;
            LookingForTeams = true;
            ValidUrl = false;
            WaitingForTeam = false;

            TeamName = "not set";
            SelectedMatch = 0;
            TeamUrl = "";
            T1Text = "";
            T2Text = "";

            T1Ban1 = "mirage";
            T1Ban2 = "vertigo";
            T2Ban1 = "dust2";
            T2Ban2 = "nuke";
            T2Pick1 = "inferno";
            T1Pick1 = "train";

            UpperTeamName = "T1";
            LowerTeamName = "T2";

            MessageQueue = new SnackbarMessageQueue();
            AllTeams = new List<int>();
            AllMatches = new List<int>();
            MatchTeamIsT1Table = new Dictionary<string, Tuple<int, bool>>();
            ThisSeasonOpponents = new ObservableCollection<ListBoxItem>();
            for (int i = 0; i < 8; i++)
            {
                ThisSeasonOpponents.Add(new ListBoxItem {Content = "T__" + i});
            }
        }

        private void SetPickBanSimulation(IReadOnlyList<string> list)
        {
            T1Ban1 = list[0];
            T2Ban1 = list[1];
            T2Ban2 = list[2];
            T1Ban2 = list[3];
            T2Pick1 = list[4];
            T1Pick1 = list[5];
        }

        private void PrintResultsToConsole()
        {
            DataModel.BatchPickBan(T1Text, true);
            DataModel.BatchPickBan(T2Text, false);

            Dictionary<Map, int> bans = _dm.Get(DecisionType.Ban);
            Dictionary<Map, int> pick = _dm.Get(DecisionType.Pick);
            Dictionary<Map, int> pool = _dm.GetMapHierachy();
            Dictionary<Map, int> rel = DataModel.GetRelativeStrength(_ourMapPool, pool);

            List<Map> mapPool = rel.OrderByDescending(x => x.Value).Select(s => s.Key).ToList();

            string results = "";
            string banner = "Map \t\tP\tB\tPool\tRel" + Environment.NewLine + "==================================" + Environment.NewLine;
            ResultText = banner;
            Debug.WriteLine(banner);
            foreach (Map item in mapPool)
            {
                bans.TryGetValue(item, out int oBan);
                pick.TryGetValue(item, out int oPick);
                pool.TryGetValue(item, out int oPool);
                rel.TryGetValue(item, out int relValue);

                results += item + Space;
                if (item != Map.Overpass)
                {
                    results += Space;
                }


                results += oPick + Space;
                results += oBan + Space;
                results += oPool + Space;
                results += relValue.ToString().PadLeft(3);
                ResultText += results + Environment.NewLine;
                results = "";
            }
        }

        #region Member

        private readonly DataModel _dm;

        private int _teamId;

        #endregion

        #region Properties

        private bool _lookingForTeams;

        public bool LookingForTeams
        {
            get => _lookingForTeams;
            set => SetField(ref _lookingForTeams, value);
        }

        private bool _isWorking;

        public bool IsWorking
        {
            get => _isWorking;
            set => SetField(ref _isWorking, value);
        }

        private bool _WaitingForTeam;

        public bool WaitingForTeam
        {
            get => _WaitingForTeam;
            set => SetField(ref _WaitingForTeam, value);
        }

        private bool _validUrl;

        public bool ValidUrl
        {
            get => _validUrl;
            set => SetField(ref _validUrl, value);
        }

        private Dictionary<string, Tuple<int, bool>> _matchTeamIsT1Table;

        public Dictionary<string, Tuple<int, bool>> MatchTeamIsT1Table
        {
            get => _matchTeamIsT1Table;
            set => SetField(ref _matchTeamIsT1Table, value);
        }

        private string _teamShort;

        public string TeamShort
        {
            get => _teamShort;
            set => SetField(ref _teamShort, value);
        }

        private string _teamName;

        public string TeamName
        {
            get => _teamName;
            set => SetField(ref _teamName, value);
        }

        private string _teamUrl;

        public string TeamUrl
        {
            get => _teamUrl;
            set => ChangeT1(ref _teamUrl, value);
        }

        private void ChangeT1(ref string teamUrl, string value)
        {
            SetField(ref teamUrl, value);
            if (value !="")
            {
                UpdateTeam();
            }
            else
            {
                ValidUrl = false;
            }
        }

        private List<int> _allMatches;

        public List<int> AllMatches
        {
            get => _allMatches;
            set => SetField(ref _allMatches, value);
        }

        private List<int> _allTeams;

        public List<int> AllTeams
        {
            get => _allTeams;
            set => SetField(ref _allTeams, value);
        }

        private int _selectedMatch;

        public int SelectedMatch
        {
            get => _selectedMatch;
            set => ChangeMatch(ref _selectedMatch, value);
        }

        private void ChangeMatch(ref int selectedMatch, int value)
        {
            SetField(ref selectedMatch, value);
            OnPropertyChanged("SelectedMatch");
            Debug.WriteLine($"Changing from {selectedMatch} to {value}");
            UpdatePickBan();
        }

        private ObservableCollection<ListBoxItem> _thisSeasonOpponents;

        public ObservableCollection<ListBoxItem> ThisSeasonOpponents
        {
            get => _thisSeasonOpponents;
            set => SetField(ref _thisSeasonOpponents, value);
        }


        private string _upperTeamName;

        public string UpperTeamName
        {
            get => _upperTeamName;
            set => SetField(ref _upperTeamName, value);
        }

        private string _lowerTeamName;

        public string LowerTeamName
        {
            get => _lowerTeamName;
            set => SetField(ref _lowerTeamName, value);
        }


        private List<KeyValuePair<Map, int>> _ourMapPool;

        private bool _ist1;

        public bool IsT1
        {
            get => _ist1;
            set => SetField(ref _ist1, value);
        }


        private string _t1Pick1;

        public string T1Pick1
        {
            get => _t1Pick1;
            set => SetField(ref _t1Pick1, value);
        }

        private string _t1Ban1;

        public string T1Ban1
        {
            get => _t1Ban1;
            set => SetField(ref _t1Ban1, value);
        }

        private string _t1Ban2;

        public string T1Ban2
        {
            get => _t1Ban2;
            set => SetField(ref _t1Ban2, value);
        }

        private string _t2Pick1;

        public string T2Pick1
        {
            get => _t2Pick1;
            set => SetField(ref _t2Pick1, value);
        }

        private string _t2Ban1;

        public string T2Ban1
        {
            get => _t2Ban1;
            set => SetField(ref _t2Ban1, value);
        }

        private string _t2Ban2;

        public string T2Ban2
        {
            get => _t2Ban2;
            set => SetField(ref _t2Ban2, value);
        }


        private string _t1Text;

        public string T1Text
        {
            get => _t1Text;
            set => SetField(ref _t1Text, value);
        }

        private string _t2Text;

        public string T2Text
        {
            get => _t2Text;
            set => SetField(ref _t2Text, value);
        }

        private string _resultText;

        public string ResultText
        {
            get => _resultText;
            set => SetField(ref _resultText, value);
        }

        private SnackbarMessageQueue _messageQueue;

        public SnackbarMessageQueue MessageQueue
        {
            get => _messageQueue;
            set => SetField(ref _messageQueue, value);
        }

        #endregion

        #region CMDS

        private DelegateCommand StartPickBanCommand { get; }

        #endregion

        #region Functions

        private async void UpdateTeam()
        {

            // get the teamName
            string temp = this.SetTeam();

            try
            {
                // get the teamName
                temp = temp.Substring(0, temp.IndexOf('-'));

                _teamId = Convert.ToInt32(temp);

                LookingForTeams = true;
                IsWorking = true;
                TeamName = await NodeJsHandler.GetTeamName(_teamId);

                // get the shorthand
                TeamShort = await NodeJsHandler.GetShortHand(_teamId);

                // get the opponents teamIds
                List<int> teamIds = await NodeJsHandler.GetOpponents(_teamId);
                List<string> shortHands = new List<string>();
                foreach (int id in teamIds)
                {
                    shortHands.Add(await NodeJsHandler.GetShortHand(id));
                }

                Dictionary<string, string> order = await NodeJsHandler.GetGameDays(_teamId, TeamShort);
                teamIds = ReorderTeam(teamIds, shortHands, order).Item1;
                shortHands = ReorderTeam(teamIds, shortHands, order).Item2;

                // Update local
                ThisSeasonOpponents = new ObservableCollection<ListBoxItem>();
                foreach (string name in shortHands)
                {
                    ThisSeasonOpponents.Add(new ListBoxItem
                    {
                        Content = name,
                        IsEnabled = IsActive(name, order),
                        Width = 150
                    });
                    
                }

                SelectedMatch = GetNextMatch(order);
                new ListBoxItem {Content = ThisSeasonOpponents[SelectedMatch].Content};

                AllTeams = teamIds.ToList();

                LookingForTeams = false;
                OnPropertyChanged("LookingForTeams");

                // get the upcoming matches
                AllMatches = await NodeJsHandler.GetAllMatches(_teamId);
                MatchTeamIsT1Table = await NodeJsHandler.GetAreWeStarting(_teamId);


                // get the your own pick/ban data
                SetOurMapPool(await NodeJsHandler.GetPickBan(_teamId));

                UpdatePickBan();
            }
            catch (Exception)
            {
                LookingForTeams = false;
                IsWorking = false;
            }
        }

        private string SetTeam()
        {
            LookingForTeams = true;
            IsWorking = true;
            ValidUrl = true;
            WaitingForTeam = true;

            this.SaveUrl();

            return TeamUrl.Substring(TeamUrl.LastIndexOf('/') + 1);
        }

        /// <summary>
        ///     Returns the first index of a match that still has to be played
        ///     Ignores cancelled matches
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        private static int GetNextMatch(Dictionary<string, string> order)
        {
            for (int i = 0; i < order.Values.Count; i++)
            {
                Regex checker = new Regex(@"(0|1|2):(0|1|2)");

                if (!checker.IsMatch(order.Values.ToList()[i]))
                {
                    return i;
                }
            }

            return 0;
        }

        private static bool IsActive(string v, IReadOnlyDictionary<string, string> order)
        {
            if (!order.ContainsKey(v))
            {
                return true;
            }

            order.TryGetValue(v, out string tempResult);
            Regex checker = new Regex(@"(0|1|2):(0|1|2)");

            return !checker.IsMatch(tempResult ?? throw new InvalidOperationException()) && tempResult != "0:0";
        }

        private static Tuple<List<int>, List<string>> ReorderTeam(IReadOnlyList<int> teamIds, IList<string> shortHands,
            Dictionary<string, string> order)
        {
            List<int> newIndexList = new List<int>();
            List<string> newShorts = new List<string>();

            foreach (string item in order.Select(s => s.Key).ToList())
            {
                int index = shortHands.IndexOf(item);
                newIndexList.Add(teamIds[index]);
                newShorts.Add(item);
            }

            return new Tuple<List<int>, List<string>>(newIndexList, newShorts);
        }

        private void SetOurMapPool(IReadOnlyCollection<List<string>> list)
        {
            List<List<Decision>> ourT1 = DataModel.BatchPickBan(GetPickBanBlock(list.First()), true);
            List<List<Decision>> ourT2 = DataModel.BatchPickBan(GetPickBanBlock(list.Last()), false);

            List<KeyValuePair<Map, int>> ourPicks = _dm.CreateHierarchy(ourT1, ourT2);
            _ourMapPool.ToList().ForEach(s => Debug.WriteLine($"{s.ToString()}"));
            Debug.WriteLine("Look at the data");
            Debug.WriteLine("-------------------");
            _ourMapPool = ourPicks.OrderBy(s => s.Value).ToList();
            _ourMapPool.ToList().ForEach(s => Debug.WriteLine($"{s.ToString()}"));
        }

        private async void UpdatePickBan()
        {
            IsWorking = true;

            if (AllTeams == null || AllTeams.Count < SelectedMatch || AllMatches.Count < SelectedMatch ||
                SelectedMatch == -1)
            {
                return;
            }

            int nextOpponent = AllTeams[SelectedMatch];
            string nameOfTheBeast = await NodeJsHandler.GetTeamName(nextOpponent);


            if (!MatchTeamIsT1Table.ContainsKey(nameOfTheBeast))
            {
                return;
            }

            // set t1
            MatchTeamIsT1Table.TryGetValue(nameOfTheBeast, out Tuple<int, bool> t1);
            if (t1 != null)
            {
                IsT1 = t1.Item2;
            }

            // set names
            if (IsT1)
            {
                UpperTeamName = TeamShort;
                LowerTeamName = ThisSeasonOpponents[SelectedMatch].Content.ToString();
            }
            else
            {
                LowerTeamName = TeamShort;
                UpperTeamName = ThisSeasonOpponents[SelectedMatch].Content.ToString();
            }

            List<List<string>> pickBan = await NodeJsHandler.GetPickBan(nextOpponent);
            T1Text = GetPickBanBlock(pickBan[0]);

            T2Text = GetPickBanBlock(pickBan[1]);

            _dm.SetPickBanData(T1Text, T2Text);

            StartPickBanCommand.Execute(new object());

            ValidUrl = false;
        }

        private static string GetPickBanBlock(IEnumerable<string> temp)
        {
            return temp.Aggregate("", (current, item) => current + item + Environment.NewLine);
        }


        protected void Enqueue(object content, bool dismiss = false)
        {
            if (!dismiss)
            {
                MessageQueue.Enqueue(content);
            }
            else
            {
                MessageQueue.Enqueue(content, "Dismiss", () => { });
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            //C# 6 null-safe operator. No need to check for event listeners
            //If there are no listeners, this will be a noop
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // C# 5 - CallMemberName means we don't need to pass the property's name
        private void SetField<T>(ref T field, T value,
            [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            if (propertyName=="LookingForTeams")
            {
                Debug.WriteLine($"LookingsForTeams : {value}");
            }
            OnPropertyChanged(propertyName);
        }

        #endregion
    }
}