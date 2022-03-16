using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TwitchChatMessage : MonoBehaviour
{
    private TwitchIRC IRC;

    public Chatter latestChatter;
    public GameObject chatMessageObject;
    public List<GameObject> messages = new List<GameObject>();
    private Vector3 newestpos;
    private float newMessHeight = 80f;
    private GameObject latestMessageObj;
    //private GameObject gm;
    //private GameManager gmScript;
    public List<string> commandStrings = new List<string>();
    public List<string> returnStrings = new List<string>();
    public string returnResponse;
    private bool waitingToRespond;

    // Start is called before the first frame update
    void Start()
    {
        // This is done just for the sake of simplicity,
        // In your own script, you should instead have a reference 
        // to the TwitchIRC component (inspector)
        IRC = GameObject.Find("TwitchIRC").GetComponent<TwitchIRC>();
        //gm = GameObject.Find("GameManager");
        //gmScript = gm.GetComponent<GameManager>();
        waitingToRespond = false;
        // Add an event listener for new chat messages
        IRC.newChatMessageEvent.AddListener(NewMessage);

        newestpos = new Vector3(0, 0, 0);
        latestMessageObj = chatMessageObject;
    }

    private void Update()
    {
        if (messages.Count > 0 && latestMessageObj != messages[messages.Count - 1])
        {
            latestMessageObj = messages[messages.Count - 1];

            if (latestMessageObj.GetComponent<RectTransform>().localPosition.y < 80f)
            {
            //UPDATE POSITIONS OF THE OLDER CHAT OBJS
                foreach (GameObject mess in messages)
                {
                    float hupdate = newMessHeight;

                    if (mess != latestMessageObj)
                    {
                        hupdate += (latestMessageObj.GetComponent<RectTransform>().rect.height - newMessHeight);
                    }

                    Vector3 newPosY = new Vector3(0, mess.GetComponent<RectTransform>().anchoredPosition3D.y + hupdate, 0);
                    mess.GetComponent<RectTransform>().anchoredPosition3D = newPosY;

                    Debug.Log("Message #" + messages.IndexOf(mess) + " height: " + mess.GetComponent<RectTransform>().rect.height);
                }
            }
        }

        if (!waitingToRespond && returnStrings.Count > 0)
        {
            waitingToRespond = true;
            int maxMessageSize = 450;
            //Concatenate responses
            for (int i = returnStrings.Count - 1; i >= 0; i--)
            {
                //Can't add any more to one response? YEET.
                if (maxMessageSize < (returnResponse.Length + returnStrings[i].Length))
                {
                    break;
                }
                returnResponse += returnStrings[i];
                if (returnStrings.Count > 1)
                {
                    returnResponse += " ";
                }
                returnStrings.RemoveAt(i);
            }
            StartCoroutine("ReturnMessage");
        }
        
    }

    //ADD NEW CHAT OBJ TO LIST
    void PushMessage(GameObject mo)
    {
        //ADD NEW CHAT OBJ TO ARRAY
        messages.Add(mo);
    }

    //GET RID OF THE OLDEST CHAT OBJ IN LIST
    void PopMessage()
    {
        GameObject oldest = messages[0];
        messages.Remove(oldest);
        GameObject.Destroy(oldest);

    }

    //RETURNS A NEW CHAT OBJ
    GameObject CreateMessageObject(string mess)
    {
        GameObject o = GameObject.Instantiate(chatMessageObject);
        o.GetComponent<RectTransform>().SetParent(this.transform);
        o.GetComponent<RectTransform>().anchoredPosition3D = newestpos;
        o.GetComponentInChildren<Text>().text = mess;
        o.GetComponent<RectTransform>().ForceUpdateRectTransforms();
        return o;
    }

    // This gets called whenever a new chat message is received
    public void NewMessage(Chatter chatter)
    {
        //no moonrunes allowed
        if (!chatter.IsDisplayNameFontSafe())
        {
            return;
        }

        bool isCommand = false;
        foreach(string c in commandStrings)
        {
            if (chatter.message.Contains(c))
            {
                isCommand = true;
                break;
            }
        }

        if (isCommand)
        {
            HandleCommand(chatter);
        }
        else
        {
            //too many messages, pop off the oldest
            if (messages.Count > 17)
            {
                PopMessage();
            }

            // Get chatter's name color (RGBA Format)
            //
            //Color nameColor = chatter.GetRGBAColor();

            //Creates object with text, using colored login followed by message
            GameObject newChatObj = CreateMessageObject(ChatMessage(chatter));

            //ADD NEW OBJ TO LIST
            PushMessage(newChatObj);
        }

        //Debug.Log(messages.Count);
        // Save latest chatter object
        // This is just to show how the Chatter object looks like inside the Inspector
        latestChatter = chatter;
    }

    /// <summary>
    /// Formats chat messages to include the user's login color.
    /// </summary>
    /// <param name="ch"></param>
    /// <returns></returns>
    string ChatMessage(Chatter ch)
    {
        string m;
        m = @"<color=#" + ColorUtility.ToHtmlStringRGB(ch.GetRGBAColor()).ToLower() + ">" + ch.login + @"</color>: " + ch.message;
        return m;
    }

    /// <summary>
    /// Called when someone types a command in twitch chat.
    /// </summary>
    /// <param name="ch"></param>
    void HandleCommand(Chatter ch)
    {
        string command;
        List<string> vars = new List<string>();
        if (ch.message.Contains(" "))
        {
            command = ch.message.Split(' ')[0];
            int tempi = ch.message.IndexOf(' ') + 1;
            string tempt = ch.message.Substring(tempi);
            string[] tempvars = tempt.Split(' ');
            foreach (string v in tempvars)
            {
                vars.Add(v);
            }
        }
        else
        {
            command = ch.message;
        }

        string m = "";
        // CHECK WHICH TYPE OF COMMAND
        
        switch (command)
        {
            
            case "!bet":
                if (!PokeGen.Instance.isBettingActive)
                {
                    m = ch.login + ", bets are currently closed.";
                    returnStrings.Add(m);
                    break;
                }
                int userwager;
                if (vars.Count == 2 && int.TryParse(vars[0], out userwager))
                {
                    HandleChatBet(ch.login, userwager, vars[1]);
                }
                else
                {
                    m = ch.login + ", try betting on a team by using the following format: !bet (wager) (team).";
                    returnStrings.Add(m);
                    break;
                }
                //m = ch.login + ", betting is currently busted and being worked on, sorry.";
                //returnStrings.Add(m);
                break;

            case "!allin":
                if (!PokeGen.Instance.isBettingActive)
                {
                    m = ch.login + ", bets are currently closed.";
                    returnStrings.Add(m);
                    break;
                }
                //int userallin;
                //if (vars.Count == 1 && int.TryParse(vars[0], out userallin))
                //{
                //    HandleChatBet(ch.login, userallin, vars[1]);
                //}
                //else
                //{
                //    m = ch.login + ", you can bet all your points on a team by using the following format: !allin (team).";
                //    returnStrings.Add(m);
                //    break;
                //}
                //break;
                m = ch.login + ", this command is currently busted, sorry.";
                returnStrings.Add(m);
                break;

            case "!bal":
                m = ch.login + ", your balance is " + GameManager.Instance.GetUserBalance(ch.login) + "p (Spendable: " + GameManager.Instance.GetUserSpendableBalance(ch.login) + "p).";
                returnStrings.Add(m);
                break;
            case "!fight":
                
                if (PokeGen.Instance.isTournamentActive)
                {
                    m = ch.login + ", there is already a tournament started.";
                    returnStrings.Add(m);
                    break;
                }
                if (PokeGen.Instance.CheckForFighterSubmitted(ch.login))
                {
                    m = ch.login + ", you have already entered into the fighter pool.";
                    returnStrings.Add(m);
                    break;
                }
                if (vars.Count > 0)
                {
                    string lowertype = vars[0].ToLower();
                    if (PokeGen.Instance.validPokeTypes.Contains(lowertype))
                    {
                        PokeGen.Instance.AddFighter(ch.login, lowertype);
                    }
                    else
                    {
                        m = ch.login + ", sorry, that command is busted at the moment. Try just using !fight.";
                        returnStrings.Add(m);
                        break;
                    }
                }
                else
                {
                    PokeGen.Instance.AddFighter(ch.login, "random");
                }
                break;
            case "!help":
                m = ch.login + ", for more info about what's happening here, please try scrolling down the page.";
                returnStrings.Add(m);
                break;
            case "!commands":
                m = "Commands: !bal || !allin || !fight || !help || !bet";
                returnStrings.Add(m);
                break;
            //case "!buy":
            //
            //    break;
            default:
                m = ch.login + ", that is not a recognized command.";
                returnStrings.Add(m);
                //IRC.SendChatMessage(m);
                break;

        }
    }

    /// <summary>
    /// Concatenates messages together to send responses back in chunk that fit max char limit.
    /// </summary>
    /// <returns></returns>
    IEnumerator ReturnMessage()
    {
        if (returnResponse != "")
        {
            IRC.SendChatMessage(returnResponse);
        }
        yield return new WaitForSeconds(2);
        
        returnResponse = "";
        waitingToRespond = false;
    }

    void HandleChatBet(string user, int wager, string team)
    {
        string convertedTeam = HandleTeamName(team);
        if (convertedTeam == "teamerror")
        {
            string response = user + ", something was wrong with your bet. Try again.";
            returnStrings.Add(response);
        }
        else
        {
            if (GameManager.Instance.ActiveTeams[0] == convertedTeam)
            {
                string response = GameManager.Instance.AddNewBetLeft(user, wager);
                returnStrings.Add(response);
            }
            else
            {
                string response = GameManager.Instance.AddNewBetRight(user, wager);
                returnStrings.Add(response);
            }
        }
    }

    string HandleTeamName(string m)
    {
        if (GameManager.Instance.ActiveTeams[0] == m || GameManager.Instance.ActiveTeams[1] == m)
        {
            return m;
        }
        switch (m)
        {
            case "left":
                return GameManager.Instance.ActiveTeams[0];
            case "right":
                return GameManager.Instance.ActiveTeams[1];
            case "random":
                int r = UnityEngine.Random.Range(0, 2);
                return GameManager.Instance.ActiveTeams[r];
            default:
                Debug.LogError("The following team doesn't exist: " + m);
                return "teamerror";
        }

    }
    //Debug.Log(
    //    "<color=cyan>New chatter object received!</color>"
    //    + " Chatter's name: " + chatter.tags.displayName
    //    + " Chatter's message: " + chatter.message);

    // Here are some examples on how you could use the chatter objects...

    //if (chatter.tags.displayName == "Lexone")
    //    Debug.Log("Chat message was sent by Lexone!");

    //if (chatter.HasBadge("subscriber"))
    //    Debug.Log("Chat message sender is a subscriber");

    //if (chatter.HasBadge("moderator"))
    //    Debug.Log("Chat message sender is a channel moderator");

    //if (chatter.MessageContainsEmote("25")) //25 = Kappa emote ID
    //    Debug.Log("Chat message contained the Kappa emote");

    //if (chatter.message == "!join")
    //    Debug.Log(chatter.tags.displayName + " said !join");
}
