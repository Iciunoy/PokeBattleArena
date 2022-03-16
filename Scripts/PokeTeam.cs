using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokeTeam : MonoBehaviour
{
    public int teamNumber;
    public List<GameObject> teamPokesObjects = new List<GameObject>();
    private List<Poke> teamPokes = new List<Poke>();
    public string formattedTeam { get; private set; }
    //private GameObject gm;

    // Start is called before the first frame update
    void Start()
    {
        //gm = GameObject.Find("GameManager").gameObject;
        //gm.GetComponent<PokeGen>().AssignTeamObject(this.gameObject);
        PokeGen.Instance.AssignTeamObject(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddPokeToTeam(Poke p)
    {
        //Debug.Log("TRYING TO GET POKE FROM POKEDEX AND GIVE TO IMG");
        teamPokes.Add(p);
        //Debug.Log("WE GOT IT BOYS");
        int i = teamPokes.Count - 1;
        teamPokesObjects[i].gameObject.SetActive(true);
        teamPokesObjects[i].GetComponent<ShowPoke>().GivePokeFromTeam(p);
        //Debug.Log("WE GAVE IT TO THE IMG");
    }

    public void ClearTeam()
    {
        foreach (GameObject go in teamPokesObjects)
        {
            teamPokes.Clear();
            go.SetActive(false);

        }
    }

    /// <summary>
    /// Return a full list of all pokemon in the team as a packed-format string
    /// </summary>
    /// <returns></returns>
    public string PokeTeamToString()
    {
        formattedTeam = "";
        foreach (Poke p in teamPokes)
        {
            if (formattedTeam == "")
            {
                string fp = p.Formatted();
                formattedTeam += fp;
            }
            else
            {
                formattedTeam += "]";
                string fp = p.Formatted();
                formattedTeam += fp;
            }
        }
        return formattedTeam;
    }

    public List<int> GetTeamPokeNums()
    {
        List<int> nums = new List<int>();
        foreach(Poke p in teamPokes)
        {
            int n = p.Num;
            nums.Add(n);
        }
        return nums;
    }
}
