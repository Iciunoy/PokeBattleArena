using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Learnset
{
    public string species;
    public List<string> AllMoves { get; private set; }

    public Learnset()
    {
        AllMoves = new List<string>();
    }

    public void AssignLearnset(string p, List<string> ms)
    {
        species = p;
        AllMoves = ms;
    }

}


