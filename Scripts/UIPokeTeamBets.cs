using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPokeTeamBets : MonoBehaviour
{
    private Color cRef;
    private int teamNumRef;
    private int leftOrRightNum;
    public bool isLeft;
    public GameObject tnameTextObj;
    public GameObject oddsTopTextObj;
    public GameObject oddsBotTextObj;
    public List<GameObject> teamPokeUIObjs = new List<GameObject>();
    private List<Poke> teamPokesRef = new List<Poke>();
    private float timerEnd;

    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("DOING THE THING NOW");
        if (isLeft)
        {
            
            PokeGen.Instance.showBettableTeamObjLeft = this.gameObject;
            leftOrRightNum = 0;
        }
        else
        {
            PokeGen.Instance.showBettableTeamObjRight = this.gameObject;
            leftOrRightNum = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (PokeGen.Instance.isBettingActive && Time.unscaledTime > timerEnd)
        {
            timerEnd = Time.unscaledTime + 2f;
            DelayedUpdate();
        }
    }

    public void UpdateTeams(List<Poke> pTeam, int tNum)
    {
        teamPokesRef = pTeam;
        teamNumRef = tNum;
        cRef = PokeGen.Instance.TeamColor(tNum);
        string tn = "TEAM " + GameManager.Instance.ActiveTeams[leftOrRightNum].ToUpper();
        UpdateTeamText(tnameTextObj, tn, cRef);
        teamPokeUIObjs[0].GetComponent<ShowTeamPoke>().ShowPokeInfo(pTeam[0], cRef);
        teamPokeUIObjs[1].GetComponent<ShowTeamPoke>().ShowPokeInfo(pTeam[1], cRef);
        teamPokeUIObjs[2].GetComponent<ShowTeamPoke>().ShowPokeInfo(pTeam[2], cRef);
        //teamPokeUIObjs[0].GetComponent<ShowTeamPoke>().ShowPokeInfo()
        DelayedUpdate();
    }

    void DelayedUpdate()
    {
        int betnum = GameManager.Instance.GetNumberOfBets(isLeft);
        int bettotal = GameManager.Instance.GetTeamPool(isLeft);
        float betper = GameManager.Instance.GetOddsPercent(isLeft);
        float betscale = GameManager.Instance.GetOddsMultiplier(isLeft);
        string newOddsTop = betnum + " bets for " + bettotal + "p";
        string newOddsBot = "(" + (float)betper + "%, " + (float)betscale + "x)";
        UpdateTeamText(oddsTopTextObj, newOddsTop, cRef);
        UpdateTeamText(oddsBotTextObj, newOddsBot, cRef);
    }
    //IEnumerator UpdateOddsText()
    //{
    //    int betnum = GameManager.Instance.GetNumberOfBets(isLeft);
    //    int bettotal = GameManager.Instance.GetTeamPool(isLeft);
    //    float betper = GameManager.Instance.GetOddsPercent(isLeft);
    //    float betscale = GameManager.Instance.GetOddsMultiplier(isLeft);
    //    string newOddsTop = betnum + " bets for " + bettotal + "p";
    //    string newOddsBot = betnum + "(" + betper + "%, " + betscale + "x)";
    //    UpdateTeamText(oddsTopTextObj, newOddsTop, cRef);
    //    UpdateTeamText(oddsBotTextObj, newOddsBot, cRef);
    //    yield return new WaitForSeconds(2);
    //    DelayedUpdate();
    //}

    public void UpdateTeamText(GameObject txtObj, string t, Color c)
    {
        Text txt = txtObj.GetComponent<Text>();
        txt.text = t;
        txt.color = c;
    }
}
