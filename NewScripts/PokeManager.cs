using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using AsyncAwaitBestPractices;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.VisualBasic.Devices;
using Microsoft.VisualBasic.Logging;
using Npgsql;
using PBA.Properties;
using static System.Windows.Forms.DataFormats;

namespace PBA
{
    public class PokeManager
    {

        /// <summary>
        /// THESE ARE FROM GAMEMANAGER
        /// </summary>

        public Thread mainThread;
        public Stopwatch tournamentStopwatch = new Stopwatch();
        public bool isGamePaused { get; private set; }
        //public TextAsset gameStateLog;
        //public TextAsset battleNumLog;
        //public TextAsset currentTeamNamesLog;
        //public TextAsset pokeBattleRoomLog;
        public string LatestUpdateText { get; private set; }
        private string isGameActive;
        public string gameState { get; private set; }
        public string[] ActiveTeams { get; private set; }
        public int CurrentRoomBattleNumber { get; private set; }
        private string lastReadReturn;
        private string currentLogJsonData;
        public string LastWinner { get; private set; }
        public Dictionary<string, int> CurrentBetsLeft = new Dictionary<string, int>();
        public Dictionary<string, int> CurrentBetsRight = new Dictionary<string, int>();
        public bool AreBetsOpen { get; private set; }
        private Pokedex _pdex;
        private Form1 _formRef;
        public static string thisNamespacePath = @"C:\Users\xxthe\OneDrive\Desktop\PokeBattle\PBA\PBA";
        public static string resourcePath = @"C:\Users\xxthe\OneDrive\Desktop\PokeBattle\PBA\PBA\Resources\";
        public static string resourcePathRepo = @"C:\Users\xxthe\source\repos\PBA\PBA\Resources\";
        public static string archiveLogPath = @"C:\Users\xxthe\OneDrive\Desktop\PokeBattle\archive\Logs\";
        public string latestLogDirPath { get; private set; }

        /// <summary>
        /// THESE ARE FROM POKEGEN
        /// </summary>

        List<string> teamStrings = new List<string>();
        private string CurrentChampTeamString;
        //public TextAsset champTeamsLog;
        //public TextAsset teamsLog;

        //Lists of pokes by team
        public List<Poke> TeamRed = new List<Poke>();
        public List<Poke> TeamBlue = new List<Poke>();
        public List<Poke> TeamYellow = new List<Poke>();
        public List<Poke> TeamGreen = new List<Poke>();
        public List<Poke> TeamAqua = new List<Poke>();
        public List<Poke> TeamPurple = new List<Poke>();
        public List<Poke> TeamPink = new List<Poke>();
        public List<Poke> TeamOrange = new List<Poke>();
        public List<Poke> TeamChamps = new List<Poke>();
        /// <summary>
        /// Spots left over in the team generation
        /// </summary>
        public Dictionary<int, PokeTeam> pokeTeams = new Dictionary<int, PokeTeam>();
        public int RemainingSpots { get; private set; }
        /// <summary>
        /// List containing strings of valid pokemon types
        /// </summary>
        public List<string> validPokeTypes = new List<string>();
        /// <summary>
        /// List of string arrays which contain fighter username and pokemon type(or random)
        /// </summary>
        public List<string[]> fighters = new List<string[]>();
        private List<string> fightersSubmitted = new List<string>();
        private int[] teamNums = new int[8] { 1, 2, 3, 4, 5, 6, 7, 8 };
        private List<int> remainingTeamNums = new List<int>();
        private int[] upcomingTeamNums;
        public Poke LastPokeMade { get; private set; }
        public bool isTournamentActive { get; private set; }
        public bool isBattleActive { get; private set; }
        public bool isBettingActive { get; private set; }
        public int TimeRemainingInt { get; private set; }
        private float TimeRemainingTarget;
        public string TimeRemainingText { get; private set; }
        public int CurrentBattleNumberInTournament { get; private set; }
        private List<BattleLog> CurrentTournamentBattleLogs = new List<BattleLog>();
        private List<string> CurrentTournamentBattleLogJSONs = new List<string>();
        public string CurrentTournamentLogPath { get; private set; }
        public int CurrentTournamentNumber { get; private set; }
        public int[] CurrentTournamentBattleNumbers { get; private set; }

        // --------------------------------------------------------GAME METHODS-----------------------------------------------
        //--------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// PokeManager constructor
        /// </summary>
        /// <param name="f"></param>
        /// <param name="pdexref"></param>
        public PokeManager(Form1 f, Pokedex pdexref)
        {
            _formRef = f;
            _pdex = pdexref;
            ActiveTeams = new string[2];
            CurrentRoomBattleNumber = CheckBattleNumberLog();
            CurrentTournamentNumber = 1;
            CurrentTournamentBattleNumbers = new int[8];
            for (int i = 0; i < 9; i++)
            {
                PokeTeam newPokeTeam = new PokeTeam(i + 1);
                pokeTeams.Add(i + 1, newPokeTeam);
            }

            //MOVED THIS TO LAUNCHER START BUTTON
            //mainThread = new Thread(new ThreadStart(ClearPokeTeams));
            //mainThread.Start();
            //tournamentStopwatch = new Stopwatch();
            //tournamentStopwatch.Start();
        }

        public void StartFromLauncher()
        {
            Debug.WriteLine("IM IN THE MAINFRAME");
            mainThread = new Thread(new ThreadStart(ClearPokeTeams));
            mainThread.Start();
            tournamentStopwatch.Start();
        }

        /// <summary>
        /// Called at the start of the program to check where things were left off
        /// </summary>
        int CheckBattleNumberLog()
        {
            int i;
            // **NEW**
            string path = thisNamespacePath + @"\Resources\DataFiles\CurrentBattleNumber.txt";
            StreamReader reader = new StreamReader(path);
            string istring = reader.ReadLine();
            reader.Close();
            if (int.TryParse(istring, out i))
            {
                System.Diagnostics.Debug.WriteLine("BATTLE NUMBER: " + i);
                return i;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ERROR READING BATTLENUMBERLOG FILE");
                return 0;
            }

        }

        /// <summary>
        /// Takes team name as a string and returns the team number
        /// </summary>
        /// <param name="tn"></param>
        /// <returns></returns>
        public int TeamNameToNum(string tn)
        {
            int newtn = 0;
            switch (tn)
            {
                case "PBATeamRed":
                    newtn = 1;
                    break;
                case "PBATeamBlue":
                    newtn = 2;
                    break;
                case "PBATeamYellow":
                    newtn = 3;
                    break;
                case "PBATeamGreen":
                    newtn = 4;
                    break;
                case "PBATeamAqua":
                    newtn = 5;
                    break;
                case "PBATeamPurple":
                    newtn = 6;
                    break;
                case "PBATeamPink":
                    newtn = 7;
                    break;
                case "PBATeamOrange":
                    newtn = 8;
                    break;
                case "PBATeamChamps":
                    newtn = 9;
                    break;
                case "red":
                    newtn = 1;
                    break;
                case "blue":
                    newtn = 2;
                    break;
                case "yellow":
                    newtn = 3;
                    break;
                case "green":
                    newtn = 4;
                    break;
                case "aqua":
                    newtn = 5;
                    break;
                case "purple":
                    newtn = 6;
                    break;
                case "pink":
                    newtn = 7;
                    break;
                case "orange":
                    newtn = 8;
                    break;
                case "champs":
                    newtn = 9;
                    break;
                default:
                    //Debug.WriteLine(tn + ": WTF THAT ISNT A TEAM. TRY AGAIN");

                    break;
            }
            return newtn;
        }

        /// <summary>
        /// Picks a random pokemon to give to a team during team gen
        /// </summary>
        /// <param name="pokeType"></param>
        /// <returns></returns>
        public Poke RandomPoke(string pokeType)
        {
            Poke p;
            if (pokeType == "random")
            {
                p = _pdex.GetRandomPoke();
            }
            else
            {
                p = _pdex.GetRandomPokeOfType(pokeType);
            }
            return p;
        }

        /// <summary>
        /// Call with team num to give UI text the correct color
        /// </summary>
        /// <param name="teamNum"></param>
        /// <returns></returns>
        public Color TeamColor(int teamNum)
        {
            Color c = Color.FromArgb(255, 255, 255);
            switch (teamNum)
            {
                case 1:
                    c = Color.FromArgb(255, 0, 0);
                    break;
                case 2:
                    c = Color.FromArgb(0, 0, 255);
                    break;
                case 3:
                    c = Color.FromArgb(0, 255, 0);
                    break;
                case 4:
                    c = Color.FromArgb(255, 255, 0);
                    break;
                case 5:
                    c = Color.FromArgb(255, 0, 255);
                    break;
                case 6:
                    c = Color.FromArgb(0, 255, 255);
                    break;
                case 7:
                    c = Color.FromArgb(255, 170, 0);
                    break;
                case 8:
                    c = Color.FromArgb(255, 0, 170);
                    break;
                case 9:
                    c = Color.FromArgb(101, 67, 33);
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("SOMETHING BROKE THATS NOT A TEAM NUMBER");
                    break;
            }
            return c;
        }

        /// <summary>
        /// Call with team num to give string Team "Color"
        /// </summary>
        /// <param name="tn"></param>
        /// <returns></returns>
        public string TeamNumToName(int tn)
        {
            string newtn = "PBATeam";
            switch (tn)
            {
                case 1:
                    newtn += "Red";
                    break;
                case 2:
                    newtn += "Blue";
                    break;
                case 3:
                    newtn += "Yellow";
                    break;
                case 4:
                    newtn += "Green";
                    break;
                case 5:
                    newtn += "Aqua";
                    break;
                case 6:
                    newtn += "Purple";
                    break;
                case 7:
                    newtn += "Pink";
                    break;
                case 8:
                    newtn += "Orange";
                    break;
                case 9:
                    newtn += "Champs";
                    break;
                default:
                    //System.Diagnostics.Debug.WriteLine("WTF THAT ISNT A TEAM. TRY AGAIN");

                    break;
            }
            return newtn;
        }

        /// <summary>
        /// Returns a list of Pokes based on the team number
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        List<Poke> GetPokeListByTeamNum(int t)
        {
            List<Poke> tList = new List<Poke>();
            switch (t)
            {
                case 1:
                    tList = TeamRed;
                    break;
                case 2:
                    tList = TeamBlue;
                    break;
                case 3:
                    tList = TeamYellow;
                    break;
                case 4:
                    tList = TeamGreen;
                    break;
                case 5:
                    tList = TeamAqua;
                    break;
                case 6:
                    tList = TeamPurple;
                    break;
                case 7:
                    tList = TeamPink;
                    break;
                case 8:
                    tList = TeamOrange;
                    break;
                case 9:
                    tList = TeamChamps;
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("THAT ISNT A TEAM WTF");
                    break;
            }
            return tList;
        }

        /// <summary>
        /// Returns a list of Pokes based on the team name
        /// </summary>
        /// <param name="team"></param>
        /// <returns></returns>
        List<Poke> GetPokeTeamFromString(string team)
        {

            List<Poke> returnpokes = new List<Poke>();
            if (CurrentChampTeamString.Length > 10)
            {
                //1.NICKNAME|2.SPECIES|3.ITEM|4.ABILITY|5.MOVES|6.NATURE|7.EVS|8.GENDER|9.IVS|10.SHINY|11.LEVEL|12.HAPPINESS,POKEBALL,HIDDENPOWERTYPE
                //important indices: 1,2,3,4,5,7
                string[] cpokes = CurrentChampTeamString.Split(']');

                foreach (string cpoke in cpokes)
                {
                    string[] vals = cpoke.Split('|');
                    string cpSpecies;
                    string[] cpokeMoves = vals[4].Split(',');
                    if (vals[1].Length > 1)
                    {
                        cpSpecies = vals[1];
                    }
                    else
                    {
                        cpSpecies = vals[0];
                    }
                    Poke pref = _pdex.pokes[cpSpecies];
                    Poke champ = new Poke(_pdex.pokes[cpSpecies].Num, vals[0], cpSpecies, vals[2], vals[3], vals[5], pref.Types, pref.Stats, pref.Evs, pref.Gender, pref.Ivs, false);
                    champ.AssignMoveset(cpokeMoves);
                    returnpokes.Add(champ);
                }
            }


            return returnpokes;
        }

        /// <summary>
        /// Battle Number (1-8), Left team = 0, right team = 1
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="leftright"></param>
        public int GetCurrentBattleTeam(int battle, int leftright)
        {
            int tn = 0;
            switch (battle)
            {
                case 1:
                    if (leftright == 0)
                    {
                        tn = upcomingTeamNums[0];
                    }
                    if (leftright == 1)
                    {
                        tn = upcomingTeamNums[1];
                    }
                    break;
                case 2:
                    if (leftright == 0)
                    {
                        tn = upcomingTeamNums[2];
                    }
                    if (leftright == 1)
                    {
                        tn = upcomingTeamNums[3];
                    }
                    break;
                case 3:
                    if (leftright == 0)
                    {
                        tn = upcomingTeamNums[4];
                    }
                    if (leftright == 1)
                    {
                        tn = upcomingTeamNums[5];
                    }
                    break;
                case 4:
                    if (leftright == 0)
                    {
                        tn = upcomingTeamNums[6];
                    }
                    if (leftright == 1)
                    {
                        tn = upcomingTeamNums[7];
                    }
                    break;
                case 5:
                    if (leftright == 0)
                    {
                        tn = upcomingTeamNums[8];
                    }
                    if (leftright == 1)
                    {
                        tn = upcomingTeamNums[9];
                    }
                    break;
                case 6:
                    if (leftright == 0)
                    {
                        tn = upcomingTeamNums[10];
                    }
                    if (leftright == 1)
                    {
                        tn = upcomingTeamNums[11];
                    }
                    break;
                case 7:
                    if (leftright == 0)
                    {
                        tn = upcomingTeamNums[12];
                    }
                    if (leftright == 1)
                    {
                        tn = upcomingTeamNums[13];
                    }
                    break;
                case 8:
                    if (leftright == 0)
                    {
                        tn = upcomingTeamNums[14];
                    }
                    if (leftright == 1)
                    {
                        tn = upcomingTeamNums[15];
                    }
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("THAT ISNT A TEAM WTF");
                    break;
            }
            return tn;
        }




        //----------------------------------------------------------MAIN GAME CYCLE FUNCTIONS------------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------------------

        
        /// <summary>
        /// Runs the command to make the cmd line actually run the python script
        /// </summary>
        public async Task run_cmd()
        {
            Debug.WriteLine("This is when run_cmd started: ");// + Time.fixedTime);
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Users\xxthe\AppData\Local\Programs\Python\Python310\python.exe";
            // POSSIBLE ARGUMENT LOCATIONS:
            //C:\Users\xxthe\OneDrive\Desktop\PokeBattle\PBA\PBA\Resources\Python\PokeBattleTeamGen.py
            //C:\Users\xxthe\source\repos\PBA\PBA\Resources\Python\PokeBattleTeamGen.py
            start.Arguments = thisNamespacePath + @"\Resources\Python\PokeBattleTeamGen.py";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardInput = true;
            start.RedirectStandardError = true;
            start.ErrorDialog = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardError)
                {

                    //process.WaitForExit();
                    string result;
                    result = await reader.ReadToEndAsync();

                    Debug.WriteLine(result);
                }
            }
            Debug.WriteLine("This is when run_cmd ended: ");// + Time.fixedTime);

        }

        /// <summary>
        /// Runs the CMD to open the pokemonshowdown URL and show the battle
        /// </summary>
        public async Task run_cmd_url()
        {
            Debug.WriteLine("This is when run_cmd_url started: ");// + Time.fixedTime);
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\\Users\\xxthe\\AppData\\Local\\Programs\\Python\\Python310\\python.exe";
            // POSSIBLE ARGUMENT LOCATIONS:
            //C:\Users\xxthe\OneDrive\Desktop\PokeBattle\PBA\PBA\Resources\Python\OpenChromeWindow.py
            //C:\Users\xxthe\source\repos\PBA\PBA\Resources\Python\OpenChromeWindow.py
            start.Arguments = thisNamespacePath + @"\Resources\Python\OpenChromeWindow.py";
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.ErrorDialog = true;
            int timeout = 600000;

            StringBuilder output = new StringBuilder();
            StringBuilder error = new StringBuilder();

            using (Process process = Process.Start(start))
            using (AutoResetEvent outputWaitHandle = new AutoResetEvent(false))
            using (AutoResetEvent errorWaitHandle = new AutoResetEvent(false))
            {
                process.OutputDataReceived += (sender, e) => {
                    if (e.Data == null)
                    {
                        outputWaitHandle.Set();
                    }
                    else
                    {
                        output.AppendLine(e.Data);
                    }
                };
                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data == null)
                    {
                        errorWaitHandle.Set();
                    }
                    else
                    {
                        error.AppendLine(e.Data);
                    }
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (process.WaitForExit(timeout) &&
                    outputWaitHandle.WaitOne(timeout) &&
                    errorWaitHandle.WaitOne(timeout))
                {
                    // Process completed. Check process.ExitCode here.
                    Debug.WriteLine("PROCESS COMPLETED, MADE IT TO HERE.");
                    lastReadReturn = "results";
                }
                else
                {
                    // Timed out.
                    Debug.WriteLine("TIMED OUT I GUESS.");
                }
            }

            Debug.WriteLine("This is when run_cmd_url ended: ");

        }


        /// <summary>
        /// TO DO LATER..... RUNS THRU ON STARTUP TO MAKE SURE VARIABLES ARE CORRECT
        /// </summary>
        //public void StartProgram()
        //{
        //    StartupTasks().SafeFireAndForget();
        //}
        //public async Task StartupTasks()
        //{
        //    await Task.Run(() => WriteStartupTeamStrings());
        //    await Task.Delay(2000);
        //    await Task.Run(() => run_cmd());
        //    await Task.Delay(2000);
        //    int battleNumCheck;
        //}


        /// <summary>
        /// (1) Clears teams for new tournament
        /// </summary>
        /// <returns></returns>
        public void ClearPokeTeams()
        {
            System.Diagnostics.Debug.WriteLine("                  ----  1. Clearing Teams  ----  ");
            //UpdateGameStateLog("active");
            //NewUpdateText("Clearing old teams...");

            fighters.Clear();
            fightersSubmitted.Clear();
            RefreshTournamentTeams();
            AddFighter("", "Random");



            // **NEW**
            foreach (PokeTeam team in pokeTeams.Values)
            {
                team.ClearTeam();
            }

            TeamRed.Clear();
            TeamBlue.Clear();
            TeamGreen.Clear();
            TeamYellow.Clear();
            TeamPurple.Clear();
            TeamAqua.Clear();
            TeamOrange.Clear();
            TeamPink.Clear();
            RemainingSpots = 24;
            System.Diagnostics.Debug.WriteLine("                Done!");
            System.Diagnostics.Debug.WriteLine("CURRENT DOMAIN: " + System.AppDomain.CurrentDomain.FriendlyName);
            System.Diagnostics.Debug.WriteLine("BASE DIRECTORY: " + System.AppDomain.CurrentDomain.BaseDirectory);
            Thread.Sleep(3000);
            CreateTeams().SafeFireAndForget();

        }

        /// <summary>
        /// (2) Creates new teams in tournament
        /// </summary>
        /// <returns></returns>
        public async Task CreateTeams()
        {
            System.Diagnostics.Debug.WriteLine("                  ----  2. Creating Teams  ----  ");
            isTournamentActive = false;
            // ONMESSAGE: SendMessageInChat("Starting a new tournament now! Use !fight in the chat to join the battle.");

            await Task.Run(() => UpdateChampTeamData());
            RefillTeamNums();

            await Task.Run(() => _formRef.ClearFormLastPoke());

            await Task.Delay(5000);
            for (int i = 1; i < 25; i++)
            {
                await Task.Delay(500);
                await Task.Run(() => CreateNewPokeFighter());
            }
            System.Diagnostics.Debug.WriteLine("                Done!");

            await Task.Delay(10000);
            await SetUpTeams();

        }

        /// <summary>
        /// (3) Sets up strings for teams
        /// </summary>
        /// <returns></returns>
        public async Task SetUpTeams()
        {
            System.Diagnostics.Debug.WriteLine("                  ----  3. Setting Up Teams  ----  ");
            isTournamentActive = true;
            await Task.Run(() => WriteTeamStrings());
            System.Diagnostics.Debug.WriteLine("                Done!");
            await Task.Delay(2000);
            await BattleTeams();
        }

        /// <summary>
        /// (4) Runs python file to have teams battle
        /// </summary>
        /// <returns></returns>
        public async Task BattleTeams()
        {
            System.Diagnostics.Debug.WriteLine("                  ----  4. Start of Battle Teams  ----  ");
            NewUpdateText("Running battle sim...");

            for (int i = 1; i < 9; i++)
            {
                //Updates team names file for python script
                UpdateTeamNamesLog(TeamNumToName(GetCurrentBattleTeam(i, 0)), TeamNumToName(GetCurrentBattleTeam(i, 1)));
                //Updates team names file for python script
                //UpdateActiveTeamNames(GetCurrentBattleTeam(i, 0), GetCurrentBattleTeam(i, 1));
                await Task.Run(() => run_cmd());
                await Task.Delay(2000);
                CurrentTournamentBattleNumbers[i - 1] = CurrentRoomBattleNumber + i;
                await Task.Run(() => UpdateCurrentLogJSONData(CurrentRoomBattleNumber + i));
                await Task.Run(() => UpdateCurrentBattleNumberLog(CurrentRoomBattleNumber + i));
                System.Diagnostics.Debug.WriteLine("    Winner is: " + CurrentTournamentBattleLogs[i - 1].Winner);
                await Task.Run(() => AddWinnerToUpcomingTeams(i, TeamNameToNum(CurrentTournamentBattleLogs[i - 1].Winner)));
                await Task.Delay(2000);

            }

            await Task.Run(() => ArchiveTournamentLogs());

            System.Diagnostics.Debug.WriteLine("                Done!");
            System.Diagnostics.Debug.WriteLine("                Start Betting in 10 seconds");

            await Task.Delay(10000);
            await StartBetting();
        }

        /// <summary>
        /// (5) Betting sequence
        /// </summary>
        /// <returns></returns>
        public async Task StartBetting()
        {
            System.Diagnostics.Debug.WriteLine("                  ----  5. Start of Betting  ----  ");
            CurrentRoomBattleNumber++;
            UpdatePokeBattleRoomURL();
            CurrentBetsLeft.Clear();
            CurrentBetsRight.Clear();
            UpdateActiveTeamNames(GetCurrentBattleTeam(CurrentBattleNumberInTournament, 0), GetCurrentBattleTeam(CurrentBattleNumberInTournament, 1));
            UpdateGameStateLog("active");
            isBettingActive = true;
            await Task.Run(() => _formRef.HideFormsPokeGen());
            await Task.Run(() => _formRef.ShowFormsBetting());
            NewUpdateText("Now opening bets...");
            _formRef.AddMessageToTwitchBot("Betting is now open for " + ActiveTeams[0] + " vs " + ActiveTeams[1] + ". Use !bet in chat!");
            // TO DO: ALL DIS IS BETTING STUFF, NEEDS TO FIX LATER.
            // ONMESSAGE: SendMessageInChat("Betting is now open for " + ActiveTeams[0] + " vs " + ActiveTeams[1] + ". Use !bet in chat!");
            AddNewBetLeft("LeftTeamFan", GetUserBalance("LeftTeamBot"));
            AddNewBetRight("RightTeamFan", GetUserBalance("RightTeamBot"));

            //CheckBattleInfo();
            //await Task.Run(() => _formRef.ReviewTeamPokesBetting(1));
            int bettingTimer = 40;
            while(bettingTimer > 0)
            {
                await Task.Delay(1000);
                
                bettingTimer--;
                
            }
            
            NewUpdateText("Playing battle now...");

            await StartBattle();


            //System.Diagnostics.Debug.WriteLine("Start Battle in 3 seconds");

        }

        /// <summary>
        /// (6) Battle animation sequence
        /// </summary>
        /// <returns></returns>
        public async Task StartBattle()
        {
            System.Diagnostics.Debug.WriteLine("                  ----  6: Start of Battle  ----  ");
            isBattleActive = true;
            UpdateGameStateLog("active");
            //FUCK JAVASCRIPT
            //CheckBattleInfo();

            int timeout = 600000;
            var task = run_cmd_url();
            if (await Task.WhenAny(task, Task.Delay(timeout)) == task)
            {

                System.Diagnostics.Debug.WriteLine("                Done!");
                await StartResults();
            }
            else
            {

                System.Diagnostics.Debug.WriteLine("                  ----  6: End of Battle (Timeout)  ----  ");
                return;
            }


        }

        //THIS IS FUCKED ATM
        /// <summary>
        /// (7) Show results of previous battle before moving on to next (3)
        /// </summary>
        /// <returns></returns>
        public async Task StartResults()
        {
            System.Diagnostics.Debug.WriteLine("                  ----  7. Start of Results  ----  ");
            NewUpdateText("Showing battle results...");

            UpdateGameStateLog("active");
            //PayoutToWinningTeam(CheckIfLeftTeamWon());

            // ONMESSAGE: SendMessageInChat("The winner is " + LastWinner + "!");

            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            //      THIS NEEDS TO CHANGE SO BATTLES ARE DONE BEFORE TOURNAMENT STARTS
            //:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
            // THIS IS THE GRAND FINAL
            if (CurrentBattleNumberInTournament > 7)
            {
                //NEW CHAMPS?
                if (TeamNameToNum(LastWinner) != 9)
                {
                    System.Diagnostics.Debug.WriteLine("                WE HAVE A NEW CHAMP!!!");
                    System.Diagnostics.Debug.WriteLine("                ----  6. End of Tournament  ----  ");
                    UpdateLogNewChamps(teamStrings[upcomingTeamNums[14]]);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("                THE CHAMPS HAVE WON AGAIN!!!");
                    System.Diagnostics.Debug.WriteLine("                ----  6. End of Tournament  ----  ");
                    //ADD CODE HERE TO UPDATE THE CHAMPS WINNING SPREE
                }

                System.Diagnostics.Debug.WriteLine("                  Clearing Teams in 10 seconds");
                await Task.Delay(10000);
                await Task.Run(() => _formRef.ShowFormsPokeGen());
                ClearPokeTeams();
            }
            //NOT YET THE GRAND FINAL
            else
            {
                System.Diagnostics.Debug.WriteLine("                ----  6. End of Results  ----  ");
                await Task.Delay(10000);
                await StartBetting();
            }

        }

        // --------------------------------------------------------BETTING FUNCTIONS-----------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------

        public void OpenForBets(bool yesorno)
        {
            AreBetsOpen = yesorno;
        }
        /// <summary>
        /// Attempts to add chat user's wager to left team bets
        /// </summary>
        /// <param name="user"></param>
        /// <param name="wager"></param>
        /// <returns></returns>
        public string AddNewBetLeft(string user, int wager)
        {
            string response = "";
            // Has this user ever bet before?
            //if (!PlayerPrefs.HasKey(user))
            //{
            //    PlayerPrefs.SetInt(user, 200);
            //}
            int previousWager = 0;
            int userFunds = 200; // PlayerPrefs.GetInt(user);
            // Was there an active bet for other team?
            if (CurrentBetsRight.ContainsKey(user))
            {
                previousWager = CurrentBetsRight[user];
                if (wager > userFunds - (int)System.MathF.Round(previousWager * 0.2f))
                {
                    response = user + ", you don't have the funds to change your current bet to that.";
                    return response;
                }
            }
            // Are you reducing your bet on current team?
            if (CurrentBetsLeft.ContainsKey(user) && CurrentBetsLeft[user] > wager)
            {
                previousWager = CurrentBetsLeft[user];
                if (wager > userFunds - (int)System.MathF.Round(previousWager * 0.2f))
                {
                    response = user + ", you don't have the funds to change your current bet to that.";
                    return response;
                }
            }
            // Does user not have enough funds?
            if (wager > userFunds)
            {
                response = user + ", you don't have enough funds for that bet.";
                return response;
            }

            CurrentBetsLeft.Add(user, wager);
            return response;
        }
        /// <summary>
        /// Attempts to add chat user's wager to right team bets
        /// </summary>
        /// <param name="user"></param>
        /// <param name="wager"></param>
        /// <returns></returns>
        public string AddNewBetRight(string user, int wager)
        {
            string response = "";
            // Has this user ever bet before?

            int previousWager = 0;
            int userFunds = 0;// PlayerPrefs.GetInt(user);
            // Was there an active bet for other team?
            if (CurrentBetsLeft.ContainsKey(user))
            {
                previousWager = CurrentBetsLeft[user];
                if (wager > userFunds - (int)System.MathF.Round(previousWager * 0.2f))
                {
                    response = ", you don't have the funds to change your current bet to that.";
                    return response;
                }
            }
            // Are you reducing your bet on current team?
            if (CurrentBetsRight.ContainsKey(user) && CurrentBetsRight[user] > wager)
            {
                previousWager = CurrentBetsRight[user];
                if (wager > userFunds - (int)System.MathF.Round(previousWager * 0.2f))
                {
                    response = ", you don't have the funds to change your current bet to that.";
                    return response;
                }
            }
            // Does user not have enough funds?
            if (wager > userFunds)
            {
                response = ", you don't have enough funds for that bet.";
                return response;
            }

            CurrentBetsRight.Add(user, wager);

            return response;
        }
        /// <summary>
        /// Gets the current bet for the chat user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int GetCurrentBet(string user)
        {
            int userbet = 0;
            if (CurrentBetsLeft.ContainsKey(user))
            {
                userbet = CurrentBetsLeft[user];
            }
            if (CurrentBetsRight.ContainsKey(user))
            {
                userbet = CurrentBetsRight[user];
            }
            return userbet;
        }
        /// <summary>
        /// Returns the user's total balance
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int GetUserBalance(string user)
        {
            int balance = 200;
            //if (!PlayerPrefs.HasKey(user))
            //{
            //    PlayerPrefs.SetInt(user, 200);
            //}
            //balance = PlayerPrefs.GetInt(user);
            return balance;
        }
        /// <summary>
        /// Returns the user's spendable balance (total - currentbet - 200)
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public int GetUserSpendableBalance(string user)
        {
            int spendable = 200;
            //if (!PlayerPrefs.HasKey(user))
            //{
            //    PlayerPrefs.SetInt(user, 200);
            //}
            //spendable = PlayerPrefs.GetInt(user) - GetCurrentBet(user) - 200;
            return spendable;
        }
        /// <summary>
        /// Assigns new playerpref balance after battle finishes.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="difference"></param>
        void SetUserNewBalance(string user, int difference)
        {
            //int old = GetUserBalance(user);
            //int change = old + difference;
            //if (change < 200)
            //{
            //    change = 200;
            //}
            //PlayerPrefs.SetInt(user, change);
        }
        /// <summary>
        /// Returns the % of overall points bet on either the left or right team
        /// </summary>
        /// <param name="isLeftTeam"></param>
        /// <returns></returns>
        public float GetOddsPercent(bool isLeftTeam)
        {
            int total = GetTotalPool();
            int leftPool = GetTeamPool(isLeftTeam);
            if (total <= 0f)
            {
                return 0f;
            }
            float response = (float)leftPool / (float)total;
            response = (int)System.MathF.Round(response * 1000f) / 10f;
            return response;
        }
        /// <summary>
        /// Returns the point multiplier for a team if they win
        /// </summary>
        /// <param name="isLeftTeam"></param>
        /// <returns></returns>
        public float GetOddsMultiplier(bool isLeftTeam)
        {
            float teamPer = GetOddsPercent(isLeftTeam);
            if (teamPer == 0f)
            {
                return 0f;
            }
            float response = GetOddsPercent(!isLeftTeam) / teamPer;
            response = (int)System.MathF.Round(response * 100f) / 100f;
            return response;
        }
        /// <summary>
        /// Gets the total number of bets for either left or right team
        /// </summary>
        /// <param name="isLeftTeam"></param>
        /// <returns></returns>
        public int GetNumberOfBets(bool isLeftTeam)
        {

            if (isLeftTeam)
            {
                int n = 0;
                foreach (KeyValuePair<string, int> pair in CurrentBetsLeft)
                {
                    n++;
                }
                return n;
            }
            else
            {
                int n = 0;
                foreach (KeyValuePair<string, int> pair in CurrentBetsRight)
                {
                    n++;
                }
                return n;
            }

        }
        /// <summary>
        /// Gets betting pool for a specific team
        /// </summary>
        /// <param name="isLeftTeam"></param>
        /// <returns></returns>
        public int GetTeamPool(bool isLeftTeam)
        {
            int totalpool = 0;
            if (isLeftTeam)
            {
                foreach (KeyValuePair<string, int> pair in CurrentBetsLeft)
                {
                    totalpool += pair.Value;
                }
            }
            else
            {
                foreach (KeyValuePair<string, int> pair in CurrentBetsRight)
                {
                    totalpool += pair.Value;
                }
            }

            return totalpool;
        }
        /// <summary>
        /// Gets betting pool for the two combined teams
        /// </summary>
        /// <returns></returns>
        public int GetTotalPool()
        {
            int total = GetTeamPool(true) + GetTeamPool(false);
            return total;
        }
        /// <summary>
        /// Pays out the users that bet on the battle
        /// </summary>
        /// <param name="isLeftTeam"></param>
        public void PayoutToWinningTeam(bool isLeftTeam)
        {
            if (isLeftTeam)
            {
                foreach (KeyValuePair<string, int> pair in CurrentBetsLeft)
                {

                    int diff = (int)(int)System.MathF.Round(pair.Value * GetOddsMultiplier(true));
                    SetUserNewBalance(pair.Key, diff);
                }
                foreach (KeyValuePair<string, int> pair in CurrentBetsRight)
                {
                    int diff = (int)(int)System.MathF.Round(pair.Value * -1);
                    SetUserNewBalance(pair.Key, diff);
                }

            }
            else
            {
                foreach (KeyValuePair<string, int> pair in CurrentBetsRight)
                {
                    int diff = (int)(int)System.MathF.Round(pair.Value * GetOddsMultiplier(false));
                    SetUserNewBalance(pair.Key, diff);
                }
                foreach (KeyValuePair<string, int> pair in CurrentBetsLeft)
                {
                    int diff = (int)(int)System.MathF.Round(pair.Value * -1);
                    SetUserNewBalance(pair.Key, diff);
                }
            }
        }


        // --------------------------------------------------------DATA FUNCTIONS-----------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------

        public async Task CheckLastBattleLog()
        {
            string path = @"C:\Users\xxthe\OneDrive\Desktop\PokeBattle\pokemon-showdown-client-main\pokemon-showdown-client\data\pokemon-showdown\logs\lastbattle.txt";

            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine();
            writer.Close();
        }

        /// <summary>
        /// Updates the battle number and the battlenumberlog file after battle finishes
        /// </summary>
        public async Task UpdateCurrentBattleNumberLog(int battleRoomNumber)
        {
            string path = thisNamespacePath + @"\Resources\DataFiles\CurrentBattleNumber.txt";

            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(battleRoomNumber);
            writer.Close();
        }

        /// <summary>
        /// Writes updated team names to teamnames.txt to be read by python battle script
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public void UpdateTeamNamesLog(string left, string right)
        {

            // **NEW**
            string path = thisNamespacePath + @"\Resources\DataFiles\teamnames.txt";

            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(left);
            writer.WriteLine(right);
            writer.Close();
        }

        /// <summary>
        /// Updates the gamestate.txt log based on a Gamestate enum.
        /// </summary>
        /// <param name="gs"></param>
        public void UpdateGameStateLog(string activebool)
        {
            string upd = "";
            isGameActive = activebool;
            gameState = upd;

            // **NEW**
            string path = thisNamespacePath + @"\Resources\DataFiles\gameStateLog.txt";

            //Write some text to the test.txt file
            StreamWriter writer = new StreamWriter(path, false);
            writer.WriteLine(isGameActive);
            writer.WriteLine(upd);
            writer.Close();
            StreamReader reader = new StreamReader(path);
            //Print the text from the file
            //Debug.WriteLine(reader.ReadToEnd());
            reader.Close();
        }

        /// <summary>
        /// Updates the txt file to contain the current battle's full URL
        /// </summary>
        public void UpdatePokeBattleRoomURL()
        {
            string baseURL = @"C://Users//xxthe//OneDrive//Desktop//PokeBattle//pokemon-showdown-client-main//pokemon-showdown-client//testclient.html?~~localhost:8000#battle-gen3pokebattlearena-";
            string fullURL = baseURL + CurrentRoomBattleNumber;

            // **NEW**
            string path = thisNamespacePath + @"\Resources\DataFiles\pokebattleroomurl.txt";

            //Write some text to the test.txt file
            StreamWriter writer = new StreamWriter(path, false);
            writer.Write(fullURL);
            writer.Close();
            StreamReader reader = new StreamReader(path);
            //Print the text from the file
            Debug.WriteLine("Opening BattleRoom URL: " + reader.ReadToEnd());
            reader.Close();
        }

        /// <summary>
        /// Updates the search to pull the latest battle log from the servers JSON url
        /// </summary>
        private async Task UpdateCurrentLogJSONData(int num)
        {


            string foldone = DateTime.Now.ToString("yyyy-MM");
            string foldtwo = DateTime.Now.ToString("yyyy-MM-dd");
            string currpath = @"C:/Users/xxthe/OneDrive/Desktop/PokeBattle/pokemon-showdown-client-main/pokemon-showdown-client/data/pokemon-showdown/logs/" + foldone + @"/gen3pokebattlearena/" + foldtwo + @"/gen3pokebattlearena-" + num + ".log.json";
            //string currurl = @"file:///" + currpath;
            string strdata;
            using (StreamReader r = new StreamReader(currpath))
            {
                strdata = r.ReadToEnd();
            }
            try
            {
                if (File.Exists(currpath))
                {
                    System.Diagnostics.Debug.WriteLine("                  Getting log data for battle log object...  ");
                    await GetCurrentLogDataFromJSON(strdata);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to find the log file.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        /// <summary>
        /// Reads the JSON file, stores all the data in a BattleLog Object, and places object in CurrentTournamentBattleLogs list.
        /// </summary>
        /// <param name="jsfile"></param>
        /// <returns></returns>
        private async Task GetCurrentLogDataFromJSON(string jsfile)
        {
            CurrentTournamentBattleLogJSONs.Add(jsfile);
            //Dictionary<string, Learnset> learnsetsAsync = new Dictionary<string, Learnset>();
            SimpleJSON.JSONNode data = SimpleJSON.JSON.Parse(jsfile);

            BattleLog tempBattleLog;
            string bWinner = "";
            List<int> bSeed = new List<int>();
            int bTurns = -1;
            string bTeamOne = "";
            string bTeamTwo = "";
            BattleLogPoke[] bTeamOnePokes = new BattleLogPoke[3];
            BattleLogPoke[] bTeamTwoPokes = new BattleLogPoke[3];
            List<string> bInputLog = new List<string>();
            List<string> bLog = new List<string>();
            string bTimestamp = "";
            string bRoomID = "";
            string bFormat = "";

            //ASSIGN ALL OF THE VALUES IN JSON TO VARIABLES, THEN ASSIGN VARIABLES TO A BATTLE LOG
            foreach (KeyValuePair<string, SimpleJSON.JSONNode> kvp in data)
            {
                switch (kvp.Key)
                {
                    case "winner":
                        bWinner += kvp.Value;
                        break;
                    case "seed":
                        foreach (SimpleJSON.JSONNode seedlog in data["seed"])
                        {
                            bSeed.Add(seedlog);
                        }
                        break;
                    case "turns":
                        bTurns += kvp.Value;
                        break;
                    case "p1":
                        bTeamOne += kvp.Value;
                        break;
                    case "p2":
                        bTeamTwo += kvp.Value;
                        break;
                    case "p1team":
                        int i = 0;
                        foreach (SimpleJSON.JSONNode pokelog in data["p1team"])
                        {
                            string bpokeName = "";
                            string bpokeSpecies = "";
                            string bpokeItem = "";
                            string bpokeAbility = "";
                            List<string> bpokeMoves = new List<string>();
                            int[] bpokeEVs = new int[6];
                            int bpokeLevel = -1;
                            int[] bpokeIVs = new int[6];
                            foreach (KeyValuePair<string, SimpleJSON.JSONNode> statlog in pokelog)
                            {

                                switch (statlog.Key)
                                {
                                    case "name":
                                        bpokeName += statlog.Value;
                                        break;
                                    case "species":
                                        bpokeSpecies += statlog.Value;
                                        break;
                                    case "item":
                                        bpokeItem += statlog.Value;
                                        break;
                                    case "ability":
                                        bpokeAbility += statlog.Value;
                                        break;
                                    case "moves":
                                        foreach (SimpleJSON.JSONNode mlog in pokelog["moves"])
                                        {
                                            bpokeMoves.Add(mlog);
                                        }
                                        break;
                                    case "evs":
                                        int j = 0;
                                        foreach (SimpleJSON.JSONNode mstat in pokelog["evs"])
                                        {
                                            bpokeEVs[j] = (int)mstat;
                                            j++;
                                        }
                                        break;
                                    case "level":
                                        bpokeLevel = statlog.Value;
                                        break;
                                    case "ivs":
                                        int k = 0;
                                        foreach (SimpleJSON.JSONNode mstat in pokelog["ivs"])
                                        {
                                            bpokeIVs[k] = (int)mstat;
                                            k++;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            BattleLogPoke temp = new BattleLogPoke(bpokeName, bpokeSpecies, bpokeItem, bpokeAbility, bpokeMoves, bpokeEVs, bpokeLevel, bpokeIVs);
                            bTeamOnePokes[i] = temp;
                            i++;
                        }
                        break;
                    case "p2team":
                        int m = 0;
                        foreach (SimpleJSON.JSONNode pokelog in data["p1team"])
                        {
                            string bpokeName = "";
                            string bpokeSpecies = "n/a";
                            string bpokeItem = "n/a";
                            string bpokeAbility = "n/a";
                            List<string> bpokeMoves = new List<string>();
                            int[] bpokeEVs = new int[6];
                            int bpokeLevel = -1;
                            int[] bpokeIVs = new int[6];
                            foreach (KeyValuePair<string, SimpleJSON.JSONNode> statlog in pokelog)
                            {

                                switch (statlog.Key)
                                {
                                    case "name":
                                        bpokeName = statlog.Value;
                                        break;
                                    case "species":
                                        bpokeSpecies = statlog.Value;
                                        break;
                                    case "item":
                                        bpokeItem = statlog.Value;
                                        break;
                                    case "ability":
                                        bpokeAbility = statlog.Value;
                                        break;
                                    case "moves":
                                        foreach (SimpleJSON.JSONNode mlog in pokelog["moves"])
                                        {
                                            bpokeMoves.Add(mlog);
                                        }
                                        break;
                                    case "evs":
                                        int n = 0;
                                        foreach (SimpleJSON.JSONNode mstat in pokelog["evs"])
                                        {
                                            bpokeEVs[n] = (int)mstat;
                                            n++;
                                        }
                                        break;
                                    case "level":
                                        bpokeLevel = statlog.Value;
                                        break;
                                    case "ivs":
                                        int p = 0;
                                        foreach (SimpleJSON.JSONNode mstat in pokelog["ivs"])
                                        {
                                            bpokeIVs[p] = (int)mstat;
                                            p++;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            BattleLogPoke temp = new BattleLogPoke(bpokeName, bpokeSpecies, bpokeItem, bpokeAbility, bpokeMoves, bpokeEVs, bpokeLevel, bpokeIVs);
                            bTeamOnePokes[m] = temp;
                            m++;
                        }
                        break;
                    case "inputLog":
                        foreach (SimpleJSON.JSONNode bilogLine in data["inputLog"])
                        {
                            bInputLog.Add(bilogLine);
                        }
                        break;
                    case "log":
                        foreach (SimpleJSON.JSONNode blogLine in data["log"])
                        {
                            bLog.Add(blogLine);
                        }
                        break;
                    case "timestamp":
                        bTimestamp = kvp.Value;
                        break;
                    case "roomid":
                        bRoomID = kvp.Value;
                        break;
                    case "format":
                        bFormat = kvp.Value;
                        break;
                    default:
                        break;
                }



            }

            tempBattleLog = new BattleLog(bWinner, bSeed, bTurns, bTeamOne, bTeamTwo, bTeamOnePokes, bTeamTwoPokes, bInputLog, bLog, bTimestamp, bRoomID, bFormat);
            System.Diagnostics.Debug.WriteLine("ROOM: " + bRoomID + " WINNER: " + bWinner);
            CurrentTournamentBattleLogs.Add(tempBattleLog);

        }

        /// <summary>
        /// Creates directory for archieved JSON files by tournament, then creates log files
        /// </summary>
        /// <returns></returns>
        private async Task ArchiveTournamentLogs()
        {
            if (CurrentTournamentBattleLogJSONs.Count < 8)
            {
                System.Diagnostics.Debug.WriteLine("Tried archiving logs before they were done processing.");
                return;
            }

            else
            {
                //  !!! IMPORTANT READ !!! UNCOMMENT THIS SECTION WHEN FINISHED WITH EVERYTHING ELSE. NEEDS ADMIN ACCESS.

                //CreateNewArchiveDirectory(CurrentTournamentNumber);

                //string path = latestLogDirPath;

                //// The line below will create a text file, my_file.txt, in 
                //// the Text_Files folder in D:\ drive.
                //// The CreateText method that returns a StreamWriter object
                //using (StreamWriter sw = File.CreateText(path))
                //{
                //    foreach (string l in CurrentTournamentBattleLogJSONs)
                //    {
                //        sw.WriteLine(l);

                //    }
                //}
                CurrentTournamentBattleLogJSONs.Clear();
            }



        }

        private void CreateNewArchiveDirectory(int i)
        {
            string newfoldOne = DateTime.Now.ToString("yyyy-MM-dd");
            string pathOne = archiveLogPath + newfoldOne;
            string pathTwo = pathOne + @"\" + i.ToString();
            try
            {
                //Date folder exists?
                if (Directory.Exists(pathOne))
                {
                    if (Directory.Exists(pathTwo))
                    {
                        int next = i + 1;
                        CreateNewArchiveDirectory(next);
                    }
                    else
                    {
                        //Create directory for tournament
                        DirectoryInfo di = Directory.CreateDirectory(pathTwo);
                        latestLogDirPath = pathTwo;
                        System.Diagnostics.Debug.WriteLine("New Directory Created: " + pathTwo);
                        CurrentTournamentNumber = i;
                    }
                }
                else
                {
                    //Create directory for date, then enter it.
                    DirectoryInfo di = Directory.CreateDirectory(pathOne);
                    System.Diagnostics.Debug.WriteLine("New Directory Created: " + pathOne);
                    CreateNewArchiveDirectory(i);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }
            finally { }
        }

        /// <summary>
        /// Update text for UI objects
        /// </summary>
        /// <param name="ut"></param>
        public void NewUpdateText(string ut)
        {
            if (LatestUpdateText != ut)
            {
                LatestUpdateText = ut;
            }
        }

        /// <summary>
        /// Writes the team strings from list of string to the text file log
        /// </summary>
        public async Task WriteTeamStrings()
        {
            System.Diagnostics.Debug.WriteLine("                    WRITING STRINGS");
            // **NEW**
            string path = thisNamespacePath + @"\Resources\DataFiles\teams.txt";
            StreamWriter clearer = new StreamWriter(path, false);
            clearer.Write("");
            clearer.Close();
            StreamWriter writer = new StreamWriter(path, true);

            // **NEW**
            foreach (PokeTeam team in pokeTeams.Values)
            {
                if (team.teamNumber > 8)
                {
                    string ts = CurrentChampTeamString;
                    teamStrings.Add(ts);
                    writer.WriteLine(ts);
                }
                else
                {
                    string ts = team.PokeTeamToString();
                    teamStrings.Add(ts);
                    writer.WriteLine(ts);
                }

            }
            writer.Close();
        }

        /// <summary>
        /// STARTUP PROCESS ONLY: Writes the team strings from list of string to the text file log
        /// </summary>
        public async Task WriteStartupTeamStrings()
        {
            // **NEW**
            string path = thisNamespacePath + @"\Resources\DataFiles\teams.txt";
            StreamWriter clearer = new StreamWriter(path, false);
            clearer.Write("");
            clearer.Close();
            StreamWriter writer = new StreamWriter(path, true);
            string test1 = "Flareon||silkscarf|0|bite,fireblast,doubleedge,protect||1,,,,,|||||]Zangoose||silkscarf|0|doubleedge,mudslap,megapunch,substitute||1,,,,,|||||]Duskull||silkscarf|0|pursuit,taunt,return,facade||1,,,,,|||||";
            string test2 = "Mudkip||silkscarf|0|endure,icywind,rocksmash,waterpulse||1,,,,,|||||]Moltres||silkscarf|0|swagger,overheat,rest,toxic||1,,,,,|||||]Slowking||silkscarf|0|endure,bodyslam,return,psychic||1,,,,,|||||";
            writer.WriteLine(test1);
            writer.WriteLine(test2);
            writer.Close();
        }

        /// <summary>
        /// Called when restarting the tournament to get the champ team based on log
        /// </summary>
        public async Task UpdateChampTeamData()
        {
            // **NEW**
            string champpath = thisNamespacePath + @"\Resources\DataFiles\champteams.txt";
            StreamReader reader = new StreamReader(champpath);
            string cstring = "";
            while (reader.Peek() >= 0)
            {
                string newstr = reader.ReadLine();
                if (newstr.Length > 10)
                {
                    cstring = newstr;
                }
            }
            CurrentChampTeamString = cstring;
            System.Diagnostics.Debug.WriteLine("CHAMP STRING: " + CurrentChampTeamString);
            reader.Close();
            List<Poke> newChamps = GetPokeTeamFromString(CurrentChampTeamString);
            TeamChamps.Clear();
            foreach (Poke p in newChamps)
            {
                TeamChamps.Add(p);
            }
        }

        /// <summary>
        /// Call to update log with the new team champ's team string for this tournament
        /// </summary>
        /// <param name="ts"></param>
        void UpdateLogNewChamps(string ts)
        {
            // **NEW**
            string path = thisNamespacePath + @"\Resources\DataFiles\champteams.txt";
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(ts);
            writer.Close();
        }

        /// <summary>
        /// Updates the team names in the ActiveTeams array.
        /// </summary>
        /// <param name="teamNumber1-9"></param>
        /// <param name="left0 right1"></param>
        public void UpdateActiveTeamNames(int tnl, int tnr)
        {
            string newtnl = "";
            string newtnr = "";
            switch (tnl)
            {
                case 1:
                    newtnl += "red";
                    break;
                case 2:
                    newtnl += "blue";
                    break;
                case 3:
                    newtnl += "yellow";
                    break;
                case 4:
                    newtnl += "green";
                    break;
                case 5:
                    newtnl += "aqua";
                    break;
                case 6:
                    newtnl += "purple";
                    break;
                case 7:
                    newtnl += "pink";
                    break;
                case 8:
                    newtnl += "orange";
                    break;
                case 9:
                    newtnl += "champs";
                    break;
                default:
                    Debug.WriteLine("WTF THAT ISNT A TEAM. TRY AGAIN");

                    break;
            }
            switch (tnr)
            {
                case 1:
                    newtnr += "red";
                    break;
                case 2:
                    newtnr += "blue";
                    break;
                case 3:
                    newtnr += "green";
                    break;
                case 4:
                    newtnr += "yellow";
                    break;
                case 5:
                    newtnr += "purple";
                    break;
                case 6:
                    newtnr += "aqua";
                    break;
                case 7:
                    newtnr += "orange";
                    break;
                case 8:
                    newtnr += "pink";
                    break;
                case 9:
                    newtnr += "champs";
                    break;
                default:
                    Debug.WriteLine("WTF THAT ISNT A TEAM. TRY AGAIN");

                    break;
            }
            ActiveTeams[0] = newtnl;
            ActiveTeams[1] = newtnr;
            Debug.WriteLine("New teams battling: " + newtnl + " and " + newtnr);
        }



        // --------------------------------------------------------FORMS FUNCTIONS-----------------------------------------------
        //-----------------------------------------------------------------------------------------------------------------------



        // --------------------------------------------------------OTHER GAME FUNCTIONS-----------------------------------------------
        //----------------------------------------------------------------------------------------------------------------------------



        // --------------------------------------------------------ARCHIVED-----------------------------------------------
        //----------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Updates the gamestate.txt log based on a string.
        /// </summary>
        /// <param name="upd"></param>
        //public void UpdateGameStateLog(string activebool, string upd)
        //{
        //    isGameActive = activebool;
        //    gameState = upd;

        //    // **NEW**
        //    string path = thisNamespacePath + @"\Resources\DataFiles\gameStateLog.txt";

        //    //Write some text to the test.txt file
        //    StreamWriter writer = new StreamWriter(path, false);
        //    writer.WriteLine(isGameActive);
        //    writer.WriteLine(upd);
        //    writer.Close();
        //    StreamReader reader = new StreamReader(path);
        //    //Print the text from the file
        //    Debug.WriteLine(reader.ReadToEnd());
        //    reader.Close();
        //}

        /// <summary>
        /// Returns the full URL for the pokebattleroom
        /// </summary>
        /// <returns></returns>
        //public string GetPokeBattleRoomURL()
        //{
        //    string url = "http://www.google.com";
        //    // **NEW**
        //    string path = thisNamespacePath + @"\Resources\DataFiles\pokebattleroomurl.txt";
        //    StreamReader reader = new StreamReader(path);
        //    //Print the text from the file

        //    url = reader.ReadToEnd();
        //    Debug.WriteLine(reader.ReadToEnd());
        //    reader.Close();
        //    return url;
        //}

        /// <summary>
        /// Takes strings in CurrentTournamentBattleLogJSONs and concatenates them into one string.
        /// </summary>
        /// <param name="blogs"></param>
        /// <returns></returns>
        //private string TournamentLogString()
        //{
        //    string rstring = "";
        //    foreach (string l in CurrentTournamentBattleLogJSONs)
        //    {
        //        rstring += l;
        //    }
        //    return rstring;
        //}

        /// <summary>
        /// Checks if the team on the left won the previous match
        /// </summary>
        /// <returns></returns>
        //public bool CheckIfLeftTeamWon()
        //{

        //    System.Diagnostics.Debug.WriteLine("TeamNumToName: " + TeamNumToName(GetCurrentBattleTeam(CurrentBattleNumberInTournament, 0)));
        //    System.Diagnostics.Debug.WriteLine("LastWinner: " + LastWinner);
        //    //if (TeamNumToName(GetCurrentBattleTeam(CurrentBattleNumberInTournament, 0)) != LastWinner)
        //    //{
        //    //    return false;
        //    //}
        //    //else
        //    //{
        //    //    return true;
        //    //}
        //    return true;
        //}

        /// <summary>
        /// Checks if a fighter has already tried to join the tournament
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        //public bool CheckForFighterSubmitted(string c)
        //{
        //    if (fightersSubmitted.Contains(c))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}


        /// <summary>
        /// Process of creating a new fighter, either for a twitch user or for a random pokemon
        /// </summary>
        public async void CreateNewPokeFighter()
        {
            if (remainingTeamNums.Count < 1)
            {
                RefillTeamNums();
            }
            Random rando = new Random();
            int rand = rando.Next(1, remainingTeamNums.Count + 1);
            int t = remainingTeamNums[rand - 1];
            while (!remainingTeamNums.Contains(t))
            {
                Random arando = new Random();
                t = arando.Next(1, remainingTeamNums.Count + 1);
                //yield return new WaitForSeconds(0.2f);
            }
            //int tn = remainingTeamNums[t];
            // IS THERE SOMEONE WHO WANTS TO PLAY? LET THEM.
            //System.Diagnostics.Debug.WriteLine("Trying to pick team: " + t);
            //System.Diagnostics.Debug.WriteLine("Fighters to pick from: " + fighters.Count);
            if (fighters.Count > 1)
            {
                Random arando = new Random();
                int r = arando.Next(1, fighters.Count);
                string newPokeType = fighters[r][1];
                System.Diagnostics.Debug.WriteLine("MAKING A POKEMON OF TYPE: " + newPokeType);
                Poke newPoke = RandomPoke(newPokeType);
                
                newPoke.ChangeNickname(fighters[r][0]);
                newPoke.RandomMoveset(_pdex.learnsets[newPoke.Species]);
                if (remainingTeamNums.Contains(t))
                {
                    bool notDupePoke = true;

                    foreach (Poke tpo in GetPokeListByTeamNum(t))
                    {
                        if (tpo.Num == newPoke.Num)
                        {
                            notDupePoke = false;
                        }
                    }

                    if (notDupePoke)
                    {
                        AddPokeToTeam(t, newPoke);

                        //_formRef.UpdateFormLastPoke(newPoke);
                        await Task.Run(() => _formRef.UpdateFormLastPoke(newPoke));

                        fighters.RemoveAt(r);
                        remainingTeamNums.Remove(t);
                        RemainingSpots--;
                    }
                    
                }


            }
            else
            {
                Poke newPoke = RandomPoke("random");
                newPoke.ChangeNickname("");
                //System.Diagnostics.Debug.WriteLine("Adding Learnset for " + newPoke.Species);
                newPoke.RandomMoveset(_pdex.learnsets[newPoke.Species]);
                //System.Diagnostics.Debug.WriteLine("TRYING TO ADD: " + newPoke.Species);
                if (remainingTeamNums.Contains(t))
                {
                    bool notDupePoke = true;

                    foreach (Poke tpo in GetPokeListByTeamNum(t))
                    {
                        if (tpo.Num == newPoke.Num)
                        {
                            notDupePoke = false;
                        }
                    }

                    if (notDupePoke)
                    {
                        AddPokeToTeam(t, newPoke);

                        await Task.Run(() => _formRef.UpdateFormLastPoke(newPoke));

                        remainingTeamNums.Remove(t);
                        RemainingSpots--;
                    }

                        
                }

            }


            if (remainingTeamNums.Count < 1)
            {
                RefillTeamNums();
            }
        }

        /// <summary>
        /// Add a fighter from chat to list of fighters
        /// </summary>
        /// <param name="chatName"></param>
        /// <param name="pokeType"></param>
        public void AddFighter(string chatName, string pokeType)
        {
            bool isFighterTaken = false;
            foreach (string[] user in fighters)
            {
                if (user[0] == chatName)
                {
                    isFighterTaken = true;
                }
            }
            if (!isFighterTaken)
            {
                string[] t = new string[2] { chatName, pokeType };
                int i = fighters.Count;
                fighters.Add(t);
                fightersSubmitted.Add(chatName);
            }
        }

        /// <summary>
        /// Used in team gen to shuffle team picks
        /// </summary>
        void RefillTeamNums()
        {
            foreach (int i in teamNums)
            {
                remainingTeamNums.Add(i);
            }
        }

        /// <summary>
        /// Adds pokemon to teams to make them seen on stream
        /// </summary>
        /// <param name="team"></param>
        /// <param name="po"></param>
        public void AddPokeToTeam(int team, Poke po)
        {
            //teamObjects[team - 1].GetComponent<PokeTeam>().AddPokeToTeam(po);
            pokeTeams[team].AddPokeToTeam(po);
            // **NEW**
            _formRef.formsPokeTeams[team].GetNextFPoke().UpdateFormPoke(po);
            
            switch (team)
            {
                case 1:
                    if (TeamRed.Count < 3)
                    {
                        TeamRed.Add(po);
                    }
                    break;
                case 2:
                    if (TeamBlue.Count < 3)
                    {
                        TeamBlue.Add(po);
                    }
                    break;
                case 3:
                    if (TeamGreen.Count < 3)
                    {
                        TeamGreen.Add(po);
                    }
                    break;
                case 4:
                    if (TeamYellow.Count < 3)
                    {
                        TeamYellow.Add(po);
                    }
                    break;
                case 5:
                    if (TeamPurple.Count < 3)
                    {
                        TeamPurple.Add(po);
                    }
                    break;
                case 6:
                    if (TeamAqua.Count < 3)
                    {
                        TeamAqua.Add(po);
                    }
                    break;
                case 7:
                    if (TeamOrange.Count < 3)
                    {
                        TeamOrange.Add(po);
                    }
                    break;
                case 8:
                    if (TeamPink.Count < 3)
                    {
                        TeamPink.Add(po);
                    }
                    break;
                case 9:
                    if (TeamChamps.Count < 3)
                    {
                        TeamChamps.Add(po);
                    }
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("SOMETHING BROKE THATS NOT A TEAM NUMBER");
                    break;
            }
        }

        /// <summary>
        /// Call after the battle with the current tournament battle number and the winner number
        /// </summary>
        /// <param name="battle"></param>
        /// <param name="winner"></param>
        public void AddWinnerToUpcomingTeams(int battle, int winner)
        {
            System.Diagnostics.Debug.WriteLine("Adding To Winners: " + winner + "/" + TeamNumToName(winner));
            switch (battle)
            {
                case 1:
                    upcomingTeamNums[8] = winner;
                    break;
                case 2:
                    upcomingTeamNums[9] = winner;
                    break;
                case 3:
                    upcomingTeamNums[10] = winner;
                    break;
                case 4:
                    upcomingTeamNums[11] = winner;
                    break;
                case 5:
                    upcomingTeamNums[12] = winner;
                    break;
                case 6:
                    upcomingTeamNums[13] = winner;
                    break;
                case 7:
                    upcomingTeamNums[14] = winner;
                    break;
                case 8:
                    break;
                default:
                    System.Diagnostics.Debug.WriteLine("                THAT ISNT A REAL BATTLE WTF HAPPENED");
                    break;
            }
            LastWinner = TeamNumToName(winner);
        }

        /// <summary>
        /// Used to clear the winners when clearing for new tournament
        /// </summary>
        void RefreshTournamentTeams()
        {
            CurrentBattleNumberInTournament = 1;
            upcomingTeamNums = new int[16] { 1, 2, 3, 4, 5, 6, 7, 8, 0, 0, 0, 0, 0, 0, 0, 9 };
        }

        
    }
}
