using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowLastPoke : MonoBehaviour
{
    private Poke pRef;
    public List<GameObject> pokeInfo = new List<GameObject>();
    public GameObject cover;
    //private GameObject gm;

    // Start is called before the first frame update
    void Start()
    {
        //this.gameObject.SetActive(true);
        //gm = GameObject.Find("GameManager").gameObject;
        PokeGen.Instance.showNewestPokeObj = this.gameObject;
        cover.SetActive(true);
        //this.gameObject.SetActive(false);
    }

    public void LastPokeInfo(Poke p, Color tcolor)
    {
        this.gameObject.SetActive(true);
        cover.SetActive(false);
        pRef = p;
        gameObject.GetComponent<Image>().sprite = pRef.pokeSprite;
        UpdateTeamText(pokeInfo[0], pRef.Nickname + " the " + pRef.Species, tcolor);
        UpdateTeamText(pokeInfo[1], pRef.Moves[0], tcolor);
        UpdateTeamText(pokeInfo[2], pRef.Moves[1], tcolor);
        UpdateTeamText(pokeInfo[3], pRef.Moves[2], tcolor);
        UpdateTeamText(pokeInfo[4], pRef.Moves[3], tcolor);

    }

    public void UpdateTeamText(GameObject txtObj, string t, Color c)
    {
        Text txt = txtObj.GetComponent<Text>();
        txt.text = t;
        txt.color = c;
    }
    
}
