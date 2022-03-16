using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowTeamPoke : MonoBehaviour
{
    private Poke pRef;
    private Color cRef;
    public List<GameObject> pokeInfo = new List<GameObject>();
    //public GameObject cover;
    //private GameObject gm;

    // Start is called before the first frame update
    void Start()
    {
        //this.gameObject.SetActive(true);
        //gm = GameObject.Find("GameManager").gameObject;
        
        //cover.SetActive(true);
        //this.gameObject.SetActive(false);
    }

    public void ShowPokeInfo(Poke p, Color tcolor)
    {
        //this.gameObject.SetActive(true);
        //cover.SetActive(false);
        pRef = p;
        cRef = tcolor;
        pokeInfo[6].gameObject.GetComponent<Image>().sprite = pRef.pokeSprite;
        UpdateTeamText(pokeInfo[0], pRef.Nickname + " the " + pRef.Species, cRef);
        UpdateTeamText(pokeInfo[1], pRef.Moves[0], cRef);
        if (pRef.Moves.Length > 1)
        {
            UpdateTeamText(pokeInfo[2], pRef.Moves[1], cRef);
        }
        else
        {
            UpdateTeamText(pokeInfo[2], "", cRef);
        }
        if (pRef.Moves.Length > 2)
        {
            UpdateTeamText(pokeInfo[3], pRef.Moves[2], cRef);
        }
        else
        {
            UpdateTeamText(pokeInfo[3], "", cRef);
        }
        if (pRef.Moves.Length > 3)
        {
            UpdateTeamText(pokeInfo[4], pRef.Moves[3], cRef);
        }
        else
        {
            UpdateTeamText(pokeInfo[4], "", cRef);
        }

    }

    public void UpdateTeamText(GameObject txtObj, string t, Color c)
    {
        Text txt = txtObj.GetComponent<Text>();
        txt.text = t;
        txt.color = c;
    }
}
