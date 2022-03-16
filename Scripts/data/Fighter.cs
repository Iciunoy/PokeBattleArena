using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Fighter
{
    public string Name { get; private set; }
    public string PokeType { get; private set; }
    public Poke FighterPoke { get; private set; }
    public Fighter()
    {
        Name = "";
        PokeType = "Random";
    }
    public Fighter(string fname, string ftype)
    {
        Name = fname;
        PokeType = ftype;
    }
}
