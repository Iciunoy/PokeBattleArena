using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITextUpdate : MonoBehaviour
{

    Text textcomp;
    string previousText;
    List<Text> tChildren = new List<Text>();
    List<string> tUps = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        //textcomp = gameObject.GetComponent<Text>();
        previousText = "";
        foreach (Text ch in this.gameObject.transform.GetComponentsInChildren<Text>())
        {
            if (this.GetComponent<Text>() != ch)
            {
                tChildren.Add(ch);
            }
                    
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (previousText != GameManager.Instance.LatestUpdateText)
        {

            previousText = GameManager.Instance.LatestUpdateText;
            //PushUpdateText(previousText);
        }
    }

    void PushUpdateText(string s)
    {
        if (tUps.Count > 3)
        {
            tUps.RemoveAt(0);
        }
        tUps.Add(s);
        int i = 0;
        foreach (Text ch in tChildren)
        {
            if (this.GetComponent<Text>() != ch)
            {
                ch.text = tUps[i];
                i++;
            }
            
        }
    }
}
