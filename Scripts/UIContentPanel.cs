using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIContentPanel : MonoBehaviour
{
    //public GameStates.GameState currentGameState;
    //GameManager gm;
    //this.transform.parent;
    GameObject objCanvas;
    //this.gameobject
    GameObject objContentWindow;
    //children
    public List<GameObject> activeObjects = new List<GameObject>();
    public GameObject objFighters;
    private GameObject objFightersInstance = null;
    public GameObject objPickedPoke;
    private GameObject objPickedPokeInstance = null;
    public GameObject objTeams;
    private GameObject objTeamsInstance = null;
    public GameObject objBettableTeam;
    private GameObject objBettableTeamInstance = null;
    private string previousGameState;


    private void Awake()
    {
        previousGameState = "teamgen";
    }
    // Start is called before the first frame update
    void Start()
    {
        //gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        objCanvas = GameObject.Find("Canvas");
        objContentWindow = this.gameObject;
        objContentWindow.GetComponent<RectTransform>().SetParent(objCanvas.GetComponent<RectTransform>());

        //CREATE CHILDREN OBJECTS
        objFightersInstance = GameObject.Instantiate(objFighters);
        objFightersInstance.GetComponent<RectTransform>().SetParent(this.transform);
        activeObjects.Add(objFightersInstance);
        objPickedPokeInstance = GameObject.Instantiate(objPickedPoke);
        objPickedPokeInstance.GetComponent<RectTransform>().SetParent(this.transform);
        activeObjects.Add(objPickedPokeInstance);
        objTeamsInstance = GameObject.Instantiate(objTeams);
        objTeamsInstance.GetComponent<RectTransform>().SetParent(this.transform);
        activeObjects.Add(objTeamsInstance);
        objBettableTeamInstance = GameObject.Instantiate(objBettableTeam);
        objBettableTeamInstance.GetComponent<RectTransform>().SetParent(this.transform);
        activeObjects.Add(objBettableTeamInstance);
        PokeGen.Instance.showBettableTeamsObj = objBettableTeamInstance;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeUIContent()
    {
        //gm = GameObject.Find("GameManager").GetComponent<GameManager>();

        //Debug.Log("SWITCHING UI TO: " + GameManager.Instance.gameState);
        switch (GameManager.Instance.gameState)
        {
            case "starting":
                StartCoroutine("StartingContent");
                break;
            case "teamgen":
                StartCoroutine("TeamGenContent");
                break;
            case "betting":
                StartCoroutine("BettingContent");
                break;
            case "battle":
                StartCoroutine("BattleContent");
                break;
            case "results":
                StartCoroutine("ResultsContent");
                break;
            case "reloading":
                StartCoroutine("ReloadingContent");
                break;
            case "clearing":
                StartCoroutine("ClearingContent");
                break;
            case "exiting":
                Debug.Log("Instead exiting code here");
                //StartCoroutine("ExitingContent");
                break;
            default:
                Debug.LogError(GameManager.Instance.gameState + " - THIS TYPE OF UI CONTENT DOESN'T EXIST DUMMY.");
                break;
        }

    }

    void ClearActiveObjects()
    {
        //foreach (GameObject go in activeObjects)
        //{
        //    go.SetActive(false);
        //    activeObjects.Remove(go);
        //    //GameObject.Destroy(go);

        //}
        for (int i = activeObjects.Count; i > 0; i--)
        {
            activeObjects[i-1].SetActive(false);
            activeObjects.RemoveAt(i-1);
        }
    }

    IEnumerator ResultsContent()
    {
        //Debug.Log("Insert code to change to Results UI here.");
        yield return new WaitForSeconds(1);
    }
    IEnumerator BettingContent()
    {
        if (activeObjects.Count > 0)
        {
            ClearActiveObjects();
        }

        objBettableTeamInstance.SetActive(true);
        activeObjects.Add(objBettableTeamInstance);
        yield return new WaitForSeconds(1);
    }
    IEnumerator BattleContent()
    {
        //Debug.Log("Insert code to change the Battle UI here.");
        yield return new WaitForSeconds(1);
    }
    IEnumerator ReloadingContent()
    {
        yield return new WaitForSeconds(1);
    }
    IEnumerator ClearingContent()
    {
        yield return new WaitForSeconds(1);
    }
    IEnumerator StartingContent()
    {
        yield return new WaitForSeconds(1);
    }
    IEnumerator TeamGenContent()
    {
        if (activeObjects.Count < 4 && activeObjects.Count > 0)
        {
            ClearActiveObjects();
        }
        if (activeObjects.Count > 3 && activeObjects.Contains(objBettableTeamInstance))
        {
            activeObjects.Remove(objBettableTeamInstance);
        }
        

        objFightersInstance.SetActive(true);
        activeObjects.Add(objFightersInstance);
        objPickedPokeInstance.SetActive(true);
        activeObjects.Add(objPickedPokeInstance);
        objTeamsInstance.SetActive(true);
        activeObjects.Add(objTeamsInstance);



        yield return new WaitForSeconds(1);
    }
}

//WEVE BEEN HERE BEFORE
//if (previousGameState != "teamgen")
//{
//    if (objFighersInstance != null)
//    {

//    }
//    else
//    {
//        objFighersInstance = GameObject.Instantiate(objFighters);
//        objFighersInstance.GetComponent<RectTransform>().SetParent(this.transform);
//        activeObjects.Add(objFighersInstance);
//    }
//    if (objFighersInstance != null)
//    {

//    }
//    else
//    {
//        objPickedPokeInstance = GameObject.Instantiate(objPickedPoke);
//        objPickedPokeInstance.GetComponent<RectTransform>().SetParent(this.transform);
//        activeObjects.Add(objPickedPokeInstance);
//    }
//    if (objFighersInstance != null)
//    {

//    }
//    else
//    {
//        objTeamsInstance = GameObject.Instantiate(objTeams);
//        objTeamsInstance.GetComponent<RectTransform>().SetParent(this.transform);
//        activeObjects.Add(objTeamsInstance);
//    }

//}
//else
//{
//    // FIRST TIME AROUND
//    objFighersInstance = GameObject.Instantiate(objFighters);
//    objFighersInstance.GetComponent<RectTransform>().SetParent(this.transform);
//    activeObjects.Add(objFighersInstance);
//    objPickedPokeInstance = GameObject.Instantiate(objPickedPoke);
//    objPickedPokeInstance.GetComponent<RectTransform>().SetParent(this.transform);
//    activeObjects.Add(objPickedPokeInstance);
//    objTeamsInstance = GameObject.Instantiate(objTeams);
//    objTeamsInstance.GetComponent<RectTransform>().SetParent(this.transform);
//    activeObjects.Add(objTeamsInstance);
//}