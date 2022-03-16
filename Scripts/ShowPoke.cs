using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowPoke : MonoBehaviour
{
    private GameObject teamObj;
    public GameObject textObj;
    private Poke pRef;
    
    // Start is called before the first frame update
    void Start()
    {
        teamObj = this.transform.parent.gameObject;
    }

    void Update()
    {
        if (gameObject.GetComponent<SpriteRenderer>().sprite == null || gameObject.GetComponent<Image>().sprite != gameObject.GetComponent<SpriteRenderer>().sprite)
        {
            gameObject.GetComponent<SpriteRenderer>().sprite = gameObject.GetComponent<Image>().sprite;
        }
    }

    public void GivePokeFromTeam(Poke p)
    {
        //Debug.Log("TRYING TO GET POKE AND SPRITE FROM TEAM");
        pRef = p;
        gameObject.GetComponent<Image>().sprite = pRef.pokeSprite;
        textObj.GetComponent<Text>().text = pRef.Nickname;
        textObj.GetComponent<Text>().color = Color.white;
    }
}
