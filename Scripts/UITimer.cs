using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimer : MonoBehaviour
{
    //GameObject gm;
    Text textcomp;
    int newNum;
    public bool isUptime;
    string newFirstText;
    string newText;

    // Start is called before the first frame update
    void Start()
    {
        //gm = GameObject.Find("GameManager");
        textcomp = gameObject.GetComponent<Text>();
        newNum = -1;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateTimer();
    }

    void UpdateTimer()
    {
        if (isUptime)
        {
            if (newNum != GameClock.Instance.TotalUnpausedTime)
            {
                newNum = (int)GameClock.Instance.TotalUnpausedTime;
                newText = "Uptime: " + newNum + " seconds.";
                textcomp.text = newText;
            }    
        }
        else
        {
            if (newFirstText != PokeGen.Instance.TimeRemainingText)
            {
                newFirstText = PokeGen.Instance.TimeRemainingText + " \n";
            }
            if (newNum != PokeGen.Instance.TimeRemainingInt)
            {
                newNum = PokeGen.Instance.TimeRemainingInt;
                newText = newFirstText + newNum + " seconds.";
                textcomp.text = newText;
            }
        }
        
        
    }
    
}
