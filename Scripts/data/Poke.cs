using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Poke
{
    
    // Pokemon's number
    // FROM POKEDEX.JSON
    public int Num { get; private set; }
    // either the pokemon's name, or the twitch user
    public string Nickname { get; private set; }
    // string of the pokemon's name
    // FROM POKEDEX.JSON
    public string Species { get; private set; }
    // string of the item
    public string Item { get; private set; }
    // 0, 1, H, or the string
    // FROM POKEDEX.JSON
    public string Ability { get; private set; }
    // 4 moves available for battle
    // FROM LEARNSETS.JSON
    public string[] Moves { get; private set; }
    // '' = Serious
    // FROM NATURES.JSON
    public string Nature { get; private set; }
    // primary type, secondary type
    // FROM POKEDEX.JSON
    public string[] Types { get; private set; }
    // hp/atk/def/spa/spd/spe
    // FROM POKEDEX.JSON
    public int[] Stats { get; private set; }
    // 0-255, 510 total
    public int[] Evs { get; private set; }
    // M/F/N
    // FROM POKEDEX.JSON
    public string Gender { get; private set; }
    // 0-31
    public int[] Ivs { get; private set; }
    // Mythical/Legendary/Restricted
    public bool SpeciesTag { get; private set; }
    public Sprite pokeSprite { get; private set; }
    public string teamString { get; private set; }



    //Generic pokemon info to be used by the pokedex
    public Poke()
    {
        Ability = "0";
        Evs = new int[6] { 1, 0, 0, 0, 0, 0 };
        Ivs = new int[6] { 31, 31, 31, 31, 31, 31 };
        Gender = "";
        
    }

    public Poke(Poke p)
    {
        Num = p.Num;
        Nickname = p.Nickname;
        Species = p.Species;
        Item = p.Item;
        Ability = p.Ability;
        Moves = p.Moves;
        Nature = p.Nature;
        Types = p.Types;
        Stats = p.Stats;
        Evs = p.Evs;
        Gender = p.Gender;
        Ivs = p.Ivs;
        SpeciesTag = p.SpeciesTag;
        pokeSprite = p.pokeSprite;

    }
    /// <summary>
    /// CREATES A POKEMON CLASS WITH ALL THE INFO, SO WRITE STRING FOR TEAM
    /// </summary>
    /// <param name="num">Pokemon's number</param>
    /// <param name="name">SpeciesName(blank string) or TwitchUsername</param>
    /// <param name="species">Pokemon Species Name</param>
    /// <param name="item">Item Held by Pokemon</param>
    /// <param name="abil">Pokemon Ability</param>
    /// <param name="moves">4 Pokemon moves</param>
    /// <param name="nat">Pokemons Nature</param>
    /// <param name="types">PrimaryType and SecondaryType</param>
    /// <param name="stats">hp/atk/def/spa/spd/spe</param>
    /// <param name="evs">0-255, 510 total</param>
    /// <param name="gend">M/F/N</param>
    /// <param name="ivs">0-31</param>
    /// <param name="st">True if Mythical/Legendary</param>
    public Poke(int num, string name, string species, string item, string abil, string nat, string[] types, int[] stats, int[] evs, string gend, int[] ivs, bool st)
    {
        Num = num;
        Nickname = name;
        Species = species;
        Item = item;    
        Ability = abil;
        Nature = nat;
        Types = types;
        Stats = stats;
        Evs = evs;
        Gender = gend;
        Ivs = ivs;
        SpeciesTag = st;
        string spritePath = "Sprites/normal/" + Num.ToString();
        pokeSprite = Resources.Load<Sprite>(spritePath);
    }

    public void AssignSprite()
    {
        string spritePath = "Sprites/normal/" + Num.ToString();
        pokeSprite = Resources.Load<Sprite>(spritePath);
    }

    public void ChangeNickname(string ttv)
    {
        if (ttv == "")
        {
            Nickname = Species;
        }
        else
        {
            Nickname = ttv;
        }
        
    }

    /// <summary>
    /// Creates random moveset from all the moves in a learnset
    /// </summary>
    /// <param name="ls"></param>
    public void RandomMoveset(Learnset ls)
    {
        string[] ms = new string[4] { "", "", "", "" };
        int full = 0;
        while (full <= ls.AllMoves.Count && full < 4)
        {
            int r = UnityEngine.Random.Range(0, ls.AllMoves.Count);
            bool already = false;
            foreach (string s in ms)
            {
                if (s == ls.AllMoves[r])
                {
                    already = true;
                }
            }
            if (!already)
            {
                ms[full] = ls.AllMoves[r];
                full++;
            }

            if (full == ls.AllMoves.Count)
            {
                break;
            }
        }
        Moves = ms;
    }

    public void AssignMoveset(string[] newMoves)
    {
        //Debug.Log("Giving " + Species + " the following moves: ");
        //int movenum = 0;
        //foreach(string m in newMoves)
        //{
        //    Moves[movenum] = m;
        //    movenum++;
        //}
        Moves = newMoves;
    }


    public string formatted_evs
    {
        get
        {

            string f_evs = "";
            if (this.Evs.Length > 0)
            {
                foreach (int el in this.Evs)
                {
                    if (f_evs == "")
                    {
                        string sel = el.ToString();
                        if (sel != 0.ToString())
                        {
                            f_evs += sel;
                        }
                    }
                    else
                    {
                        f_evs += ",";
                        string sel = el.ToString();
                        if (sel != 0.ToString())
                        {
                            f_evs += sel;
                        }
                    }
                }
            }

            if (f_evs == ",,,,,")
            {
                return "";
            }
            return f_evs;
        }
    }

    public string formatted_ivs
    {
        get
        {

            string f_ivs = "";
            if (this.Ivs.Length > 0)
            {
                foreach (int el in this.Ivs)
                {
                    
                    if (f_ivs == "")
                    {
                        string sel = el.ToString();
                        if (sel != 31.ToString())
                        {
                            f_ivs += sel;
                        }
                    }
                    else
                    {
                        f_ivs += ",";
                        string sel = el.ToString();
                        if (sel != 31.ToString())
                        {
                            f_ivs += sel;
                        }
                    }
                    
                }
            }

            if (f_ivs == ",,,,,")
            {
                return "";
            }
            return f_ivs;
        }
    }

    public object formatted_moves
    {
        get
        {

            string f_moves = "";
            if (this.Moves.Length > 0)
            {
                foreach (string move in this.Moves)
                {
                    if (f_moves == "")
                    {
                        f_moves += move;
                    }
                    else
                    {
                        f_moves += ",";
                        f_moves += move;
                    }
                }
            }

            return f_moves;
        }
    }

    public string Formatted()
    {
        string f_poke = "";
        string scheck = "";
        if (Species != Nickname)
        {
            scheck = Species;
        }
        f_poke = Nickname + "|" + scheck + "|" + Item + "|" + Ability + "|" + formatted_moves + "|" + Nature + "|" + formatted_evs + "|" + Gender + "|" + formatted_ivs + "|||";
        return f_poke;
    }

    

}




//   TEXT POKEDEX FORMAT
//   #,Species,Type 1,Type 2,Total,HP,Attack,Defense,Sp. Atk,Sp. Def,Speed,Generation,Legendary

//   POKEMON SHOWDOWN TEAM GEN FORMAT (LEAVE THE FOLLOWING EMPTY: SHINY, LEVEL, HAPPINESS, POKEBALL, HIDDENPOWERTYPE
//   00000000|1111111|2222|3333333|44444|555555|6,6|777777|8,8|99999|10000|110000000,12000000,130000000000000]
//   NICKNAME|SPECIES|ITEM|ABILITY|MOVES|NATURE|EVS|GENDER|IVS|SHINY|LEVEL|HAPPINESS,POKEBALL,HIDDENPOWERTYPE]