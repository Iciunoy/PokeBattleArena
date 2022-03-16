using System.IO;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PokeGen : MonoBehaviour
{
    // Start is called before the first frame update
    public static PokeGen Instance { get; private set;}
    List<string> teamStrings = new List<string>();
    private string CurrentChampTeamString;
    public TextAsset champTeamsLog;
    public TextAsset teamsLog;
    //Lists of pokes by team
    public List<Poke> TeamRed = new List<Poke>();
    public List<Poke> TeamBlue = new List<Poke>();
    public List<Poke> TeamGreen = new List<Poke>();
    public List<Poke> TeamYellow = new List<Poke>();
    public List<Poke> TeamPurple = new List<Poke>();
    public List<Poke> TeamAqua = new List<Poke>();
    public List<Poke> TeamOrange = new List<Poke>();
    public List<Poke> TeamPink = new List<Poke>();
    public List<Poke> TeamChamps = new List<Poke>();
    public List<GameObject> teamObjects = new List<GameObject>();
    public GameObject showNewestPokeObj;
    public GameObject showBettableTeamsObj;
    public GameObject showBettableTeamObjLeft;
    public GameObject showBettableTeamObjRight;
    /// <summary>
    /// Spots left over in the team generation
    /// </summary>
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
    //EVENT STUFF
    UnityEvent EventClearPokeTeams = new UnityEvent();
    UnityEvent EventCreateTeams = new UnityEvent();
    UnityEvent EventSetUpTeams = new UnityEvent();
    UnityEvent EventBattleTeams = new UnityEvent();
    UnityEvent EventStartBetting = new UnityEvent();
    UnityEvent EventStartBattle = new UnityEvent();
    UnityEvent EventStartResults = new UnityEvent();
    UnityEvent EventCreateNewPokeFighter = new UnityEvent();
    TwitchChatMessage chatbotRef;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Start of PokeGen");
        EventClearPokeTeams.AddListener(CallClearPokeTeams);
        EventCreateTeams.AddListener(CallCreateTeams);
        EventSetUpTeams.AddListener(CallSetUpTeams);
        EventBattleTeams.AddListener(CallBattleTeams);
        EventStartBetting.AddListener(CallStartBetting);
        EventStartBattle.AddListener(CallStartBattle);
        EventStartResults.AddListener(CallStartResults);
        EventCreateNewPokeFighter.AddListener(CreateNewPokeFighter);
        upcomingTeamNums = new int[16] { 1,2,3,4,5,6,7,8,0,0,0,0,0,0,0,9 };
        isBattleActive = false;
        isBettingActive = false;
        EventClearPokeTeams.Invoke();
        chatbotRef = GameObject.Find("UIChat").GetComponent<TwitchChatMessage>();
    }

    //void Update()
    //{
        
    //}

    public void CallClearPokeTeams()
    {
        StartCoroutine("ClearPokeTeams");
    }
    /// <summary>
    /// (1) Clears teams for new tournament
    /// </summary>
    /// <returns></returns>
    IEnumerator ClearPokeTeams()
    {
        Debug.Log("Start of Clearing Teams");
        GameManager.Instance.UpdateGameStateLog("active", "teamgen");
        GameManager.Instance.NewUpdateText("Clearing old teams...");
        
        fighters.Clear();
        fightersSubmitted.Clear();
        RefreshTournamentTeams();
        AddFighter("", "Random");

        foreach (GameObject go in teamObjects)
        {
            go.GetComponent<PokeTeam>().ClearTeam();
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
        yield return null;
        GameClock.Instance.CreateTimer(3f, EventCreateTeams, true);
        //***********************************
        //     THIS IS A WAITING LOOP
        //***********************************

        // Float time which we will wait for
        //float waitTime = 5f;
        //Debug.Log("Creating Teams in 5 seconds");
        //// Create a timer which will trigger an event after the given time
        //TimeRemainingText = "Creating Teams in: ";
        //// Updates the time based on the unpaused game clock
        //TimeRemainingTarget = GameClock.Instance.TotalUnpausedTime + waitTime;
        //// Updates the int which will be displayed on the UI
        //TimeRemainingInt = Mathf.RoundToInt(TimeRemainingTarget - GameClock.Instance.TotalUnpausedTime);
        //// Waits in a loop until the time is finished, then the timer runs the next event
        //while (TimeRemainingInt > 0)
        //{
        //    TimeRemainingInt = Mathf.RoundToInt(TimeRemainingTarget - GameClock.Instance.TotalUnpausedTime);
        //}

        //EventCreateTeams.Invoke();

    }
    
    
    public void CallCreateTeams()
    {
        StartCoroutine("CreateTeams");
    }
    /// <summary>
    /// (2) Creates new teams in tournament
    /// </summary>
    /// <returns></returns>
    IEnumerator CreateTeams()
    {
        Debug.Log("Start of Creating Teams");
        showBettableTeamsObj.SetActive(false);
        isTournamentActive = false;
        GameManager.Instance.NewUpdateText("Creating new teams...");
        SendMessageInChat("Starting a new tournament now! Use !fight in the chat to join the battle.");
        UpdateChampTeamStuff();
        RefillTeamNums();
        //Debug.Log("Creating pokes for teams now");
        //Create a new timer to add fighters for the new tournament
        for (int i = 1; i < 25; i++)
        {
            GameClock.Instance.CreateTimer(30 + (i*5f), EventCreateNewPokeFighter, false);
        }
        
        yield return null;

        //***********************************
        //     THIS IS A WAITING LOOP
        //***********************************

        // Float time which we will wait for
        float waitTime = 160f;
        Debug.Log("Battling Teams in 160 seconds");
        // Create a timer which will trigger an event after the given time
        GameClock.Instance.CreateTimer(waitTime, EventSetUpTeams, true);
        TimeRemainingText = "Tournament will start in: ";
        // Updates the time based on the unpaused game clock
        TimeRemainingTarget = GameClock.Instance.TotalUnpausedTime + waitTime;
        // Updates the int which will be displayed on the UI
        TimeRemainingInt = Mathf.RoundToInt(TimeRemainingTarget - GameClock.Instance.TotalUnpausedTime);
        // Waits in a loop until the time is finished, then the timer runs the next event
        while (TimeRemainingInt > 0)
        {
            yield return new WaitForSeconds(1f);
            TimeRemainingInt = Mathf.RoundToInt(TimeRemainingTarget - GameClock.Instance.TotalUnpausedTime);
        }
        
    }
    

    public void CallSetUpTeams()
    {
        StartCoroutine("SetUpTeams");
    }
    IEnumerator SetUpTeams()
    {
        Debug.Log("Start of Setting Up Teams");
        isTournamentActive = true;
        //UnityEngine.Debug.Log("BATTLE HAPPENS HERE");
        WriteTeamStrings();
        float waitTime = 10f;
        GameClock.Instance.CreateTimer(waitTime, EventBattleTeams, true);
        
        yield return null;
    }
    
    public void CallBattleTeams()
    {
        StartCoroutine("BattleTeams");
    }
    /// <summary>
    /// (3) Runs python file to have teams battle
    /// </summary>
    /// <returns></returns>
    IEnumerator BattleTeams()
    {
        GameManager.Instance.NewUpdateText("Running battle sim...");
        GameManager.Instance.UpdateTeamNames(TeamNumToName(GetCurrentBattleTeam(CurrentBattleNumberInTournament, 0)), TeamNumToName(GetCurrentBattleTeam(CurrentBattleNumberInTournament, 1)));
        GameManager.Instance.UpdateActiveTeamNames(GetCurrentBattleTeam(CurrentBattleNumberInTournament, 0), GetCurrentBattleTeam(CurrentBattleNumberInTournament, 1));
        GameManager.Instance.CurrentBetsLeft.Clear();
        GameManager.Instance.CurrentBetsRight.Clear();
        GameManager.Instance.AddNewBetLeft("LeftTeamBot", GameManager.Instance.GetUserBalance("LeftTeamBot"));
        GameManager.Instance.AddNewBetRight("RightTeamBot", GameManager.Instance.GetUserBalance("RightTeamBot"));
        //yield return new WaitForSeconds(2);
        //Debug.Log("BATTLING TEAMS NOW");
        GameManager.Instance.run_cmd();
        //yield return new WaitForSeconds(5);
        
        GameManager.Instance.UpdateBattleNumber();
        //AddWinnerToUpcomingTeams(CurrentBattleNumberInTournament, )
        GameManager.Instance.UpdatePokeBattleRoomURL();
        //GameManager.Instance.CheckBattleInfo();
        //Debug.Log("The winner was: " + GameManager.Instance.LastWinner);
        //AddWinnerToUpcomingTeams(CurrentBattleNumberInTournament, GameManager.Instance.TeamNameToNum(GameManager.Instance.LastWinner));
        //UnityEngine.Debug.Log("BATTLE ENDED");
        //StartCoroutine("StartBetting");
        yield return null;
        //Debug.Log("Start Betting in 3 seconds");
        GameClock.Instance.CreateTimer(10f, EventStartBetting, true);
    }
    
    
    public void CallStartBetting()
    {
        StartCoroutine("StartBetting");
    }
    /// <summary>
    /// (4) Betting sequence
    /// </summary>
    /// <returns></returns>
    IEnumerator StartBetting()
    {
        GameManager.Instance.UpdateGameStateLog("active", GameStates.GameState.betting);
        isBettingActive = true;
        GameObject.Find("UIContentWindow").GetComponent<UIContentPanel>().ChangeUIContent();
        showBettableTeamObjLeft.GetComponent<UIPokeTeamBets>().UpdateTeams(GetPokeListByTeamNum(GameManager.Instance.TeamNameToNum(GameManager.Instance.ActiveTeams[0])), GameManager.Instance.TeamNameToNum(GameManager.Instance.ActiveTeams[0]));
        showBettableTeamObjRight.GetComponent<UIPokeTeamBets>().UpdateTeams(GetPokeListByTeamNum(GameManager.Instance.TeamNameToNum(GameManager.Instance.ActiveTeams[1])), GameManager.Instance.TeamNameToNum(GameManager.Instance.ActiveTeams[1]));
        GameManager.Instance.NewUpdateText("Now opening bets...");
        SendMessageInChat("Betting is now open for " + GameManager.Instance.ActiveTeams[0] + " vs " + GameManager.Instance.ActiveTeams[1] + ". Use !bet in chat!");
        yield return new WaitForSeconds(1);
        //GameManager.Instance.CheckBattleInfo();
        //Debug.Log("The winner was: " + GameManager.Instance.LastWinner);
        AddWinnerToUpcomingTeams(CurrentBattleNumberInTournament, GameManager.Instance.TeamNameToNum(GameManager.Instance.LastWinner));
        //GameManager.Instance.AddNewBetLeft("LeftTeamBot", GameManager.Instance.GetUserBalance("LeftTeamBot"));
        //GameManager.Instance.AddNewBetRight("RightTeamBot", GameManager.Instance.GetUserBalance("RightTeamBot"));
        // THIS IS THE LOOP TO UPDATE THE UITIMER
        //Debug.Log("Upcoming Team Numbers: ");
        //foreach (int n in upcomingTeamNums)
        //{
        //    Debug.Log(n);
        //}
        //***********************************
        //     THIS IS A WAITING LOOP
        //***********************************
        yield return new WaitForSeconds(1);
        //// Float time which we will wait for
        float waitTime = 30f;
        Debug.Log("Betting ends in 30 seconds");
        // Create a timer which will trigger an event after the given time
        GameClock.Instance.CreateTimer(waitTime, EventStartBattle, true);
        TimeRemainingText = "Betting is open for: ";
        // Updates the time based on the unpaused game clock
        TimeRemainingTarget = GameClock.Instance.TotalUnpausedTime + waitTime;
        // Updates the int which will be displayed on the UI
        TimeRemainingInt = Mathf.RoundToInt(TimeRemainingTarget - GameClock.Instance.TotalUnpausedTime);
        // Waits in a loop until the time is finished, then the timer runs the next event
        while (TimeRemainingInt > 0)
        {
            yield return new WaitForSeconds(1f);
            TimeRemainingInt = Mathf.RoundToInt(TimeRemainingTarget - GameClock.Instance.TotalUnpausedTime);
        }

        GameManager.Instance.NewUpdateText("Playing battle now...");
        yield return null;
        //Debug.Log("Start Battle in 3 seconds");
        
    }


    public void CallStartBattle()
    {
        StartCoroutine("StartBattle");
    }
    /// <summary>
    /// (5) Battle animation sequence
    /// </summary>
    /// <returns></returns>
    IEnumerator StartBattle()
    {
        
        isBattleActive = true;
        GameManager.Instance.UpdateGameStateLog("active", GameStates.GameState.battle);
        //FUCK JAVASCRIPT
        GameManager.Instance.CheckBattleInfo();
        
        GameManager.Instance.run_cmd_url();
        //GameManager.Instance.StartCoroutine("WaitForResult");
        yield return null;
        //Debug.Log("The winner was: " + GameManager.Instance.LastWinner);
        AddWinnerToUpcomingTeams(CurrentBattleNumberInTournament, GameManager.Instance.TeamNameToNum(GameManager.Instance.LastWinner));
        GameClock.Instance.CreateTimer(3f, EventStartResults, true);
    }


    public void CallStartResults()
    {
        StartCoroutine("StartResults");
    }
    /// <summary>
    /// (6) Show results of previous battle before moving on to next (3)
    /// </summary>
    /// <returns></returns>
    IEnumerator StartResults()
    {
        GameManager.Instance.NewUpdateText("Showing battle results...");
        GameManager.Instance.CheckBattleInfo();
        //Debug.Log("The winner was: " + GameManager.Instance.LastWinner);
        AddWinnerToUpcomingTeams(CurrentBattleNumberInTournament, GameManager.Instance.TeamNameToNum(GameManager.Instance.LastWinner));
        //yield return new WaitForSeconds(5);
        GameManager.Instance.UpdateGameStateLog("active", GameStates.GameState.results);
        GameManager.Instance.PayoutToWinningTeam(CheckIfLeftTeamWon());
        SendMessageInChat("The winner is " + GameManager.Instance.LastWinner + "!");
        //Debug.Log("SHOW RESULTS NOW");
        CurrentBattleNumberInTournament++;
        //Debug.Log("NEXT BATTLE NUMBER: " + CurrentBattleNumberInTournament);
        // THIS IS THE GRAND FINAL
        if (CurrentBattleNumberInTournament > 8)
        {
            //NEW CHAMPS?
            if (GameManager.Instance.TeamNameToNum(GameManager.Instance.LastWinner) != 9)
            {
                //Debug.Log("WE HAVE A NEW CHAMP!!!");
                UpdateLogNewChamps( teamStrings[ upcomingTeamNums[ 14 ] ] );
            }    
            
            //Debug.Log("Clearing Teams in 10 seconds");
            GameClock.Instance.CreateTimer(10f, EventClearPokeTeams, true);
            yield return null;
        }    
        //NOT YET THE GRAND FINAL
        else
        {
            //Debug.Log("Battling Teams in 10 seconds");
            GameClock.Instance.CreateTimer(10f, EventBattleTeams, true);
            yield return null;
        }
        
        TimeRemainingText = GameManager.Instance.LastWinner + " won! Next battle begins in: ";
        TimeRemainingInt = 10;
        
        while (isBettingActive)
        {
            yield return new WaitForSeconds(1);
            if (TimeRemainingInt < 1)
            {
                isBettingActive = false;
                break;
            }
            TimeRemainingInt--;
        }

    }



    /// <summary>
    /// Process of creating a new fighter, either for a twitch user or for a random pokemon
    /// </summary>
    public void CreateNewPokeFighter()
    {
        if (remainingTeamNums.Count < 1)
        {
            RefillTeamNums();
        }

        int rand = UnityEngine.Random.Range(1, remainingTeamNums.Count + 1);
        int t = remainingTeamNums[rand - 1];
        while (!remainingTeamNums.Contains(t))
        {
            t = UnityEngine.Random.Range(1, remainingTeamNums.Count + 1);
            //yield return new WaitForSeconds(0.2f);
        }
        //int tn = remainingTeamNums[t];
        // IS THERE SOMEONE WHO WANTS TO PLAY? LET THEM.
        //Debug.Log("Trying to pick team: " + t);
        //Debug.Log("Fighters to pick from: " + fighters.Count);
        if (fighters.Count > 1)
        {
            int r = UnityEngine.Random.Range(1, fighters.Count);
            string newPokeType = fighters[r][1];
            Debug.Log("MAKING A POKEMON OF TYPE: " + newPokeType);
            Poke newPoke = RandomPoke(newPokeType);
            List<int> checknum = teamObjects[t - 1].GetComponent<PokeTeam>().GetTeamPokeNums();
            while (checknum.Contains(newPoke.Num))
            {
                newPoke = RandomPoke(newPokeType);
            }
            newPoke.ChangeNickname(fighters[r][0]);
            //newPoke.GivePokeMoves(Pokedex.learnsets[newPoke.Species].AllMoves);
            newPoke.RandomMoveset(Pokedex.learnsets[newPoke.Species]);
            if (remainingTeamNums.Contains(t))
            {
                AddPokeToTeam(t, newPoke);
                if (RemainingSpots == 24)
                {
                    showNewestPokeObj = GameObject.FindGameObjectWithTag("PokePreview").gameObject;
                }
                showNewestPokeObj.GetComponent<ShowLastPoke>().LastPokeInfo(newPoke, TeamColor(t));
                fighters.RemoveAt(r);
                remainingTeamNums.Remove(t);
                RemainingSpots--;
            }


        }
        else
        {
            Poke newPoke = RandomPoke("random");
            List<int> checknum = teamObjects[t - 1].GetComponent<PokeTeam>().GetTeamPokeNums();
            while (checknum.Contains(newPoke.Num))
            {
                newPoke = RandomPoke("random");
            }
            newPoke.ChangeNickname("");
            //newPoke.GivePokeMoves(Pokedex.learnsets[newPoke.Species].AllMoves);
            //Debug.Log("Adding Learnset for " + newPoke.Species);
            newPoke.RandomMoveset(Pokedex.learnsets[newPoke.Species]);
            //Debug.Log("TRYING TO ADD: " + newPoke.Species);
            if (remainingTeamNums.Contains(t))
            {
                AddPokeToTeam(t, newPoke);
                if (RemainingSpots == 24)
                {
                    showNewestPokeObj = GameObject.FindGameObjectWithTag("PokePreview").gameObject;
                }
                showNewestPokeObj.GetComponent<ShowLastPoke>().LastPokeInfo(newPoke, TeamColor(t));
                remainingTeamNums.Remove(t);
                RemainingSpots--;
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
        foreach(string[] user in fighters)
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
    /// Picks a random pokemon to give to a team during team gen
    /// </summary>
    /// <param name="pokeType"></param>
    /// <returns></returns>
    public Poke RandomPoke(string pokeType)
    {
        Poke p;
        if (pokeType == "random")
        {
            p = gameObject.GetComponent<Pokedex>().GetRandomPoke();
        }
        else
        {
            p = gameObject.GetComponent<Pokedex>().GetRandomPokeOfType(pokeType);
        }
        return p;
    }
    /// <summary>
    /// Adds pokemon to teams to make them seen on stream
    /// </summary>
    /// <param name="team"></param>
    /// <param name="po"></param>
    public void AddPokeToTeam(int team, Poke po)
    {
        teamObjects[team - 1].GetComponent<PokeTeam>().AddPokeToTeam(po);
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
                Debug.Log("SOMETHING BROKE THATS NOT A TEAM NUMBER");
                break;
        }
    }

    /// <summary>
    /// Assigns team object to the list of team objects
    /// </summary>
    /// <param name="o"></param>
    public void AssignTeamObject(GameObject o)
    {
        teamObjects.Add(o);
    }

    /// <summary>
    /// Writes the team strings from list of string to the text file log
    /// </summary>
    void WriteTeamStrings()
    {
        Debug.Log("WRITING STRINGS");
        string path = AssetDatabase.GetAssetPath(teamsLog);
        StreamWriter clearer = new StreamWriter(path, false);
        clearer.Write("");
        clearer.Close();
        StreamWriter writer = new StreamWriter(path, true);
        foreach (GameObject go in teamObjects)
        {
            string ts = go.GetComponent<PokeTeam>().PokeTeamToString();
            teamStrings.Add(ts);
            
            writer.WriteLine(ts);
            
        }
        
        writer.WriteLine(CurrentChampTeamString);
        writer.Close();
    }

    /// <summary>
    /// Called when restarting the tournament to get the champ team based on log
    /// </summary>
    void UpdateChampTeamStuff()
    {
        string champpath = AssetDatabase.GetAssetPath(champTeamsLog);
        StreamReader reader = new StreamReader(champpath);
        string cstring = "";
        while (reader.Peek() >= 0)
        {
            cstring = reader.ReadLine();
        }
        CurrentChampTeamString = cstring;
        reader.Close();
        List<Poke> newChamps = GetPokeTeamFromString(CurrentChampTeamString);
        TeamChamps.Clear();
        foreach(Poke p in newChamps)
        {
            TeamChamps.Add(p);
        }
    }

    /// <summary>
    /// Opens the local URL for the battle room to watch animation
    /// </summary>
    public void OpenURL()
    {
        Debug.Log("TRYING TO OPEN URL");
        string bnumstring = GameManager.Instance.BattleNumber.ToString();
        string basename = @"C://Users//Owner//Desktop//PokeBattle//pokemon-showdown-client-main//pokemon-showdown-client//testclient.html?~~localhost:8000#battle-gen3pokebattlearena-";
        string fname = basename + bnumstring;

        Application.OpenURL("file:///" + fname);
        Debug.Log(fname);
        //string tempurl = "C:/Users/Owner/Desktop/PokeBattle/pokemon-showdown-client-main/pokemon-showdown-client/testclient.html?~~localhost:8000#battle-gen3pokebattlearena-";
        //string bnumstring = gm.GetComponent<GameManager>().BattleNumber.ToString();
        //string target = $"C:\\Users\\Owner\\Desktop\\PokeBattle\\pokemon-showdown-client-main\\pokemon-showdown-client\\testclient.html?~~localhost:8000#battle-gen3pokebattlearena-{bnumstring}";
        //OpenPdfInBrowser(target, )

        Debug.Log("is this working?");
    }
    // ****** BUG ********
    // FIGURE OUT THIS BIG CLUSTERFUCK TO GET THE URL TO OPEN.
    public void OpenPdfInBrowser(string url, string localPdf)
    {
        //string body = string.Format(localHTMLbody, url);
        //File.WriteAllText(localPdf, body);
        //Application.OpenURL("file:///" + localPdf);
        //try
        //{
        //    System.Diagnostics.Process.Start(target);
        //}
        //catch (System.ComponentModel.Win32Exception noBrowser)
        //{
        //    if (noBrowser.ErrorCode == -2147467259)
        //        UnityEngine.Debug.LogError(noBrowser.Message);
        //}
        //catch (System.Exception other)
        //{
        //    UnityEngine.Debug.LogError(other.Message);
        //}
    }

    /// <summary>
    /// Call with team num to give UI text the correct color
    /// </summary>
    /// <param name="teamNum"></param>
    /// <returns></returns>
    public Color TeamColor(int teamNum)
    {
        Color c = new Color(1f, 1f, 1f);
        switch (teamNum)
        {
            case 1:
                c = new Color(1f, 0f, 0f);
                break;
            case 2:
                c = new Color(0f, 0f, 1f);
                break;
            case 3:
                c = new Color(0f, 1f, 0f);
                break;
            case 4:
                c = new Color(1f, 1f, 0f);
                break;
            case 5:
                c = new Color(1f, 0f, 1f);
                break;
            case 6:
                c = new Color(0f, 1f, 1f);
                break;
            case 7:
                c = new Color(1f, (170f / 255f), 0f);
                break;
            case 8:
                c = new Color(1f, 0f, (170f / 255f));
                break;
            case 9:
                c = new Color(101f, 67f, 33f);
                break;
            default:
                Debug.Log("SOMETHING BROKE THATS NOT A TEAM NUMBER");
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
        switch(tn)
        {
            case 1:
                newtn += "Red";
                break;
            case 2:
                newtn += "Blue";
                break;
            case 3:
                newtn += "Green";
                break;
            case 4:
                newtn += "Yellow";
                break;
            case 5:
                newtn += "Purple";
                break;
            case 6:
                newtn += "Aqua";
                break;
            case 7:
                newtn += "Orange";
                break;
            case 8:
                newtn += "Pink";
                break;
            case 9:
                newtn += "Champs";
                break;
            default:
                //Debug.Log("WTF THAT ISNT A TEAM. TRY AGAIN");
                
                break;
        }
        return newtn;
    }

    /// <summary>
    /// Call to update log with the new team champ's team string for this tournament
    /// </summary>
    /// <param name="ts"></param>
    void UpdateLogNewChamps(string ts)
    {
        string path = AssetDatabase.GetAssetPath(champTeamsLog);
        StreamWriter writer = new StreamWriter(path, true);
        writer.WriteLine(ts);
        writer.Close();
    }

    /// <summary>
    /// Call after the battle with the current tournament battle number and the winner number
    /// </summary>
    /// <param name="battle"></param>
    /// <param name="winner"></param>
    void AddWinnerToUpcomingTeams(int battle, int winner)
    {
        Debug.Log("Adding To Winners: " + winner + "/" + TeamNumToName(winner));
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
                Debug.Log("THAT ISNT A REAL BATTLE WTF HAPPENED");
                break;
        }
    }

    /// <summary>
    /// Used to clear the winners when clearing for new tournament
    /// </summary>
    void RefreshTournamentTeams()
    {
        CurrentBattleNumberInTournament = 1;
        upcomingTeamNums = new int[16] { 1, 2, 3, 4, 5, 6, 7, 8, 0, 0, 0, 0, 0, 0, 0, 9 };
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
                tList = TeamGreen;
                break;
            case 4:
                tList = TeamYellow;
                break;
            case 5:
                tList = TeamPurple;
                break;
            case 6:
                tList = TeamAqua;
                break;
            case 7:
                tList = TeamOrange;
                break;
            case 8:
                tList = TeamPink;
                break;
            case 9:
                tList = TeamChamps;
                break;
            default:
                Debug.Log("THAT ISNT A TEAM WTF");
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
                Poke pref = Pokedex.pokes[cpSpecies];
                Poke champ = new Poke(Pokedex.pokes[cpSpecies].Num, vals[0], cpSpecies, vals[2], vals[3], vals[5], pref.Types, pref.Stats, pref.Evs, pref.Gender, pref.Ivs, false);
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
    int GetCurrentBattleTeam(int battle, int leftright)
    {
        int tn = 0;
        switch(battle)
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
                Debug.Log("THAT ISNT A TEAM WTF");
                break;
        }
        return tn;
    }
    
    /// <summary>
    /// Checks if the team on the left won the previous match
    /// </summary>
    /// <returns></returns>
    public bool CheckIfLeftTeamWon()
    {
        Debug.Log("TeamNumToName: " + TeamNumToName(GetCurrentBattleTeam(CurrentBattleNumberInTournament, 0)));
        Debug.Log("LastWinner: " + GameManager.Instance.LastWinner);
        if (TeamNumToName(GetCurrentBattleTeam(CurrentBattleNumberInTournament, 0)) != GameManager.Instance.LastWinner)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    
    /// <summary>
    /// Checks if a fighter has already tried to join the tournament
    /// </summary>
    /// <param name="c"></param>
    /// <returns></returns>
    public bool CheckForFighterSubmitted(string c)
    {
        if (fightersSubmitted.Contains(c))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void SendMessageInChat(string r)
    {
        chatbotRef.returnStrings.Add(r);
    }
}