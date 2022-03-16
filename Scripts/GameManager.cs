using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Scripting.Python;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Events;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject objCanvas;
    GameStates gstates;
    public bool isGamePaused { get; private set; }
    public TextAsset gameStateLog;
    public TextAsset battleNumLog;
    public TextAsset currentTeamNamesLog;
    public TextAsset pokeBattleRoomLog;
    public string LatestUpdateText { get; private set; }
    private string isGameActive;
    public string gameState { get; private set; }
    public string[] ActiveTeams { get; private set; }
    public int BattleNumber { get; private set; }
    private string lastReadReturn;
    private string currentLogJsonUrl;
    private string currentLogJsonData;
    public string LastWinner { get; private set; }
    public Dictionary<string, int> CurrentBetsLeft = new Dictionary<string, int>();
    public Dictionary<string, int> CurrentBetsRight = new Dictionary<string, int>();
    public bool AreBetsOpen { get; private set; }
    //EVENT STUFF

    private void Awake()
    {
        Instance = this;
        isGamePaused = false;
        gstates = new GameStates();
        isGameActive = "active";
        gameState = "teamgen";
        LatestUpdateText = "";
        ActiveTeams = new string[2] { "red", "blue" };
    }
    // Start is called before the first frame update
    void Start()
    {
        //UpdateGameStateLog("active", "teamgen");
        BattleNumber = CheckBattleNumberLog();
        //string mpath = Application.dataPath;
        //UnityEngine.Debug.Log(mpath);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.P))
        {
            UnityEngine.Debug.Log("IS THE GAME PAUSED: " + isGamePaused);
            UnityEngine.Debug.Log("WHATS THE LAST READ RETURN: " + lastReadReturn);
            UnityEngine.Debug.Log("CHAMPS:");
            UnityEngine.Debug.Log(PokeGen.Instance.TeamChamps[0].Species + ", WITH MOVES: " + PokeGen.Instance.TeamChamps[0].Moves);
            UnityEngine.Debug.Log(PokeGen.Instance.TeamChamps[1].Species + ", WITH MOVES: " + PokeGen.Instance.TeamChamps[1].Moves);
            UnityEngine.Debug.Log(PokeGen.Instance.TeamChamps[2].Species + ", WITH MOVES: " + PokeGen.Instance.TeamChamps[2].Moves);
        }
        
        if (isGamePaused && lastReadReturn == "results")
        {
            isGamePaused = false;
            lastReadReturn = "";
            PokeGen.Instance.CallStartResults();
            UnityEngine.Debug.Log("WTF ITS TRYING TO GO TO RESULTS, WHY NOT???");
        }
    }

    private void OnApplicationQuit()
    {
        //isGameActive = "inactive";
        //gameState = "exiting";
        PlayerPrefs.Save();
        UpdateGameStateLog("inactive", "exiting");
        
    }

    /// <summary>
    /// Called at the start of the program to check where things were left off
    /// </summary>
    int CheckBattleNumberLog()
    {
        int i;
        string path = AssetDatabase.GetAssetPath(battleNumLog);
        StreamReader reader = new StreamReader(path);
        string istring = reader.ReadLine();
        reader.Close();
        if (int.TryParse(istring, out i))
        {
            UnityEngine.Debug.Log("BATTLE NUMBER: " + i);
            return i;
        }
        else
        {
            UnityEngine.Debug.LogError("ERROR READING BATTLENUMBERLOG FILE");
            return 0;
        }
        
    }
    
    /// <summary>
    /// Updates the battle number and the battlenumberlog file after battle finishes
    /// </summary>
    public void UpdateBattleNumber()
    {
        BattleNumber++;
        string path = AssetDatabase.GetAssetPath(battleNumLog);
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(BattleNumber);
        writer.Close();
    }
    
    public void UpdateTeamNames(string left, string right)
    {
        string path = AssetDatabase.GetAssetPath(currentTeamNamesLog);
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(left);
        writer.WriteLine(right);
        writer.Close();
    }
    
    /// <summary>
    /// Updates the gamestate.txt log based on a Gamestate enum.
    /// </summary>
    /// <param name="gs"></param>
    public void UpdateGameStateLog(string activebool, GameStates.GameState gs)
    {
        string upd = gstates.GameStateToString(gs);
        isGameActive = activebool;
        gameState = upd;
        string path = AssetDatabase.GetAssetPath(gameStateLog);
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(isGameActive);
        writer.WriteLine(upd);
        writer.Close();
        StreamReader reader = new StreamReader(path);
        //Print the text from the file
        //UnityEngine.Debug.Log(reader.ReadToEnd());
        reader.Close();
        GameObject.Find("UIContentWindow").GetComponent<UIContentPanel>().ChangeUIContent();
    }

    /// <summary>
    /// Updates the gamestate.txt log based on a string.
    /// </summary>
    /// <param name="upd"></param>
    public void UpdateGameStateLog(string activebool, string upd)
    {
        isGameActive = activebool;
        gameState = upd;
        string path = AssetDatabase.GetAssetPath(gameStateLog);
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);
        writer.WriteLine(isGameActive);
        writer.WriteLine(upd);
        writer.Close();
        StreamReader reader = new StreamReader(path);
        //Print the text from the file
        UnityEngine.Debug.Log(reader.ReadToEnd());
        reader.Close();
        GameObject.Find("UIContentWindow").GetComponent<UIContentPanel>().ChangeUIContent();
    }

    /// <summary>
    /// LoadingScene -> NewGameScene -> BettingScene -> BattleScene -> ResultsScene -> BettingScene
    /// </summary>
    /// <param name="sceneTo"></param>
    public void ChangeScene(string sceneTo)
    {
        SceneManager.LoadScene(sceneTo);
    }
    
    /// <summary>
    /// Updates the txt file to contain the current battle's full URL
    /// </summary>
    public void UpdatePokeBattleRoomURL()
    {
        string baseURL = @"C://Users//Owner//Desktop//PokeBattle//pokemon-showdown-client-main//pokemon-showdown-client//testclient.html?~~localhost:8000#battle-gen3pokebattlearena-";
        string fullURL = baseURL + BattleNumber;
        string path = AssetDatabase.GetAssetPath(pokeBattleRoomLog);
        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);
        writer.Write(fullURL);
        writer.Close();
        StreamReader reader = new StreamReader(path);
        //Print the text from the file
        UnityEngine.Debug.Log(reader.ReadToEnd());
        reader.Close();
    }
    
    /// <summary>
    /// Returns the full URL for the pokebattleroom
    /// </summary>
    /// <returns></returns>
    public string GetPokeBattleRoomURL()
    {
        string url = "http://www.google.com";
        string path = AssetDatabase.GetAssetPath(pokeBattleRoomLog);
        StreamReader reader = new StreamReader(path);
        //Print the text from the file

        url = reader.ReadToEnd();
        UnityEngine.Debug.Log(reader.ReadToEnd());
        reader.Close();
        return url;
    }
    
    /// <summary>
    /// Runs the command to make the cmd line actually run the python script
    /// </summary>
    public void run_cmd()
    {
        UnityEngine.Debug.Log("This is when run_cmd started: " + Time.fixedTime);
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = @"C:\\Users\\Owner\\AppData\\Local\\Programs\\Python\\Python310\\python.exe";
        //start.Arguments = Application.dataPath + "/PokeEnvMaster/poke-env-master/examples/pokebattlearena/PokeBattleTeamGen.py";
        start.Arguments = @"C:\\Users\\Owner\\Documents\\UnityProjects\\PokeBattleArena\\Assets\\PokeEnvMaster\\poke-env-master\\examples\\pokebattlearena\\PokeBattleTeamGen.py";
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
                string result = reader.ReadToEnd();

                UnityEngine.Debug.Log(result);
            }
        }
        UnityEngine.Debug.Log("This is when run_cmd ended: " + Time.fixedTime);
        //PythonRunner.RunFile("C:\\Users\\Owner\\Documents\\Unity Projects\\PokeBattleArena\\Assets\\PokeEnvMaster\\poke-env-master\\examples\\pokebattlearena\\PokeBattleTeamGen.py");

    }

    /// <summary>
    /// Runs the CMD to open the pokemonshowdown URL and show the battle
    /// </summary>
    public void run_cmd_url()
    {
        UnityEngine.Debug.Log("This is when run_cmd_url started: " + Time.fixedTime);
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = @"C:\\Users\\Owner\\AppData\\Local\\Programs\\Python\\Python310\\python.exe";
        start.Arguments = @"C:\\Users\\Owner\\Documents\\UnityProjects\\PokeBattleArena\\Assets\\PokeEnvMaster\\poke-env-master\\examples\\pokebattlearena\\OpenChromeWindow.py";
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.ErrorDialog = true;
        int timeout = 600;

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
                UnityEngine.Debug.Log("PROCESS COMPLETED, MADE IT TO HERE.");
                lastReadReturn = "results";
                GameManager.Instance.UpdateGameStateLog("active", GameStates.GameState.results);
            }
            else
            {
                // Timed out.
                UnityEngine.Debug.Log("TIMED OUT I GUESS.");
            }
        }

        //using (Process process = Process.Start(start))
        //{
        //    using (StreamReader reader = process.StandardOutput)
        //    {
        //        string result = reader.ReadToEnd();

        //        if (result.Contains("results"))
        //        {
        //            lastReadReturn = "results";
        //            GameManager.Instance.UpdateGameStateLog("active", GameStates.GameState.results);
        //        }
        //        reader.Close();
        //    }
        //}
        UnityEngine.Debug.Log("This is when run_cmd_url ended: " + Time.fixedTime);

    }

    IEnumerator WaitForResult()
    {
        yield return new WaitForSeconds(1);
        while (lastReadReturn != "results")
        {
            yield return new WaitForSeconds(1);
        }
        
    }
    
    /// <summary>
    /// Updates the search to pull the latest battle log from the servers JSON url
    /// </summary>
    public void CheckBattleInfo()
    {
        
        string foldone = DateTime.Now.ToString("yyyy-MM");
        string foldtwo = DateTime.Now.ToString("yyyy-MM-dd");
        string currurl = @"file:///" + @"C:/Users/Owner/Desktop/PokeBattle/pokemon-showdown-client-main/pokemon-showdown-client/data/pokemon-showdown/logs/" + foldone + @"/gen3pokebattlearena/" + foldtwo + @"/gen3pokebattlearena-" + BattleNumber + ".log.json";
        int max = 0;
        currentLogJsonUrl = currurl;
        StartCoroutine("RequestLogInfo");
        
    }

    /// <summary>
    /// Gets the log of the battle which last took place from the servers json url
    /// </summary>
    /// <returns></returns>
    IEnumerator RequestLogInfo()
    {
        //UnityEngine.Debug.Log(currentLogJsonUrl);
        UnityWebRequest request = UnityWebRequest.Get(currentLogJsonUrl);
        yield return request.SendWebRequest();
        //UnityEngine.Debug.Log("Received" + request.downloadHandler.text);
        //currentLogJsonData = JsonConvert.DeserializeObject<string>(request.downloadHandler.text);
        currentLogJsonData = request.downloadHandler.text;
        //var Rates = JsonConvert.DeserializeObject<Rates>(request.downloadHandler.text);

        //exchange = GameObject.Find("MainText").GetComponent<Text>();
        //exchange.text = Rates.ZAR.ToString();
        SimpleJSON.JSONNode data = SimpleJSON.JSON.Parse(currentLogJsonData);
        //SimpleJSON.JSONNode rstr = data[0]["winner"].Value;
        UnityEngine.Debug.Log(data[0]);
        LastWinner = data[0];
    }
    //file:///C:/Users/Owner/Desktop/PokeBattle/pokemon-showdown-client-main/pokemon-showdown-client/data/pokemon-showdown/logs/2022-02/gen3pokebattlearena/2022-02-13/gen3pokebattlearena-103.log.json
    
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
                newtnl += "green";
                break;
            case 4:
                newtnl += "yellow";
                break;
            case 5:
                newtnl += "purple";
                break;
            case 6:
                newtnl += "aqua";
                break;
            case 7:
                newtnl += "orange";
                break;
            case 8:
                newtnl += "pink";
                break;
            case 9:
                newtnl += "champs";
                break;
            default:
                UnityEngine.Debug.Log("WTF THAT ISNT A TEAM. TRY AGAIN");

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
                UnityEngine.Debug.Log("WTF THAT ISNT A TEAM. TRY AGAIN");

                break;
        }
        ActiveTeams[0] = newtnl;
        ActiveTeams[1] = newtnr;
        UnityEngine.Debug.Log("New teams battling: " + newtnl + " and " + newtnr);
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
            case "PBATeamGreen":
                newtn = 3;
                break;
            case "PBATeamYellow":
                newtn = 4;
                break;
            case "PBATeamPurple":
                newtn = 5;
                break;
            case "PBATeamAqua":
                newtn = 6;
                break;
            case "PBATeamOrange":
                newtn = 7;
                break;
            case "PBATeamPink":
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
            case "green":
                newtn = 3;
                break;
            case "yellow":
                newtn = 4;
                break;
            case "purple":
                newtn = 5;
                break;
            case "aqua":
                newtn = 6;
                break;
            case "orange":
                newtn = 7;
                break;
            case "pink":
                newtn = 8;
                break;
            case "champs":
                newtn = 9;
                break;
            default:
                //UnityEngine.Debug.Log(tn + ": WTF THAT ISNT A TEAM. TRY AGAIN");

                break;
        }
        return newtn;
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
        if (!PlayerPrefs.HasKey(user))
        {
            PlayerPrefs.SetInt(user, 200);
        }
        int previousWager = 0;
        int userFunds = PlayerPrefs.GetInt(user);
        // Was there an active bet for other team?
        if (CurrentBetsRight.ContainsKey(user))
        {
            previousWager = CurrentBetsRight[user];
            if (wager > userFunds - Mathf.RoundToInt(previousWager * 0.2f))
            {
                response = user + ", you don't have the funds to change your current bet to that.";
                return response;
            }
        }
        // Are you reducing your bet on current team?
        if (CurrentBetsLeft.ContainsKey(user) && CurrentBetsLeft[user] > wager)
        {
            previousWager = CurrentBetsLeft[user];
            if (wager > userFunds - Mathf.RoundToInt(previousWager * 0.2f))
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
        int userFunds = PlayerPrefs.GetInt(user);
        // Was there an active bet for other team?
        if (CurrentBetsLeft.ContainsKey(user))
        {
            previousWager = CurrentBetsLeft[user];
            if (wager > userFunds - Mathf.RoundToInt(previousWager * 0.2f))
            {
                response = ", you don't have the funds to change your current bet to that.";
                return response;
            }
        }
        // Are you reducing your bet on current team?
        if (CurrentBetsRight.ContainsKey(user) && CurrentBetsRight[user] > wager)
        {
            previousWager = CurrentBetsRight[user];
            if (wager > userFunds - Mathf.RoundToInt(previousWager * 0.2f))
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
        int balance;
        if (!PlayerPrefs.HasKey(user))
        {
            PlayerPrefs.SetInt(user, 200);
        }
        balance = PlayerPrefs.GetInt(user);
        return balance;
    }
    /// <summary>
    /// Returns the user's spendable balance (total - currentbet - 200)
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public int GetUserSpendableBalance(string user)
    {
        int spendable;
        if (!PlayerPrefs.HasKey(user))
        {
            PlayerPrefs.SetInt(user, 200);
        }
        spendable = PlayerPrefs.GetInt(user) - GetCurrentBet(user) - 200;
        return spendable;
    }
    
    /// <summary>
    /// Assigns new playerpref balance after battle finishes.
    /// </summary>
    /// <param name="user"></param>
    /// <param name="difference"></param>
    void SetUserNewBalance(string user, int difference)
    {
        int old = GetUserBalance(user);
        int change = old + difference;
        if (change < 200)
        {
            change = 200;
        }
        PlayerPrefs.SetInt(user, change);
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
        response = Mathf.Round(response * 1000f) / 10f;
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
        response = Mathf.Round(response * 100f) / 100f;
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
                int diff = Mathf.RoundToInt(pair.Value * GetOddsMultiplier(true));
                SetUserNewBalance(pair.Key, diff);
            }
            foreach (KeyValuePair<string, int> pair in CurrentBetsRight)
            {
                int diff = Mathf.RoundToInt(pair.Value * -1);
                SetUserNewBalance(pair.Key, diff);
            }

        }
        else
        {
            foreach (KeyValuePair<string, int> pair in CurrentBetsRight)
            {
                int diff = Mathf.RoundToInt(pair.Value * GetOddsMultiplier(false));
                SetUserNewBalance(pair.Key, diff);
            }
            foreach (KeyValuePair<string, int> pair in CurrentBetsLeft)
            {
                int diff = Mathf.RoundToInt(pair.Value * -1);
                SetUserNewBalance(pair.Key, diff);
            }
        }
    }

    

    public void PauseToggle(bool pausedState)
    {
        isGamePaused = pausedState;
    }
    
}
