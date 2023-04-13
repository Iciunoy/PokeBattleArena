using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AsyncAwaitBestPractices;

namespace PBA
{
    public class Pokedex
    {
        public List<int> bannedPokes = new List<int>() { 10, 11, 13, 14, 83, 94, 122, 129, 132, 150, 151, 175, 235, 249, 250, 251, 266, 268, 374 };
        public List<string> allowedPokeTypes = new List<string>() { "dark", "normal", "fighting", "flying", "poison", "ground", "rock", "bug", "ghost", "steel", "fire", "water", "grass", "electric", "psychic", "ice", "dragon" };
        public Dictionary<int, string> pokeSpeciesByNum = new Dictionary<int, string>();
        public Dictionary<string, List<int>> pokeNumsByType = new Dictionary<string, List<int>>();
        public Dictionary<string, Poke> pokes = new Dictionary<string, Poke>();
        public Dictionary<string, Learnset> learnsets = new Dictionary<string, Learnset>();
        //public string strLearnset;
        //public string strPokedex;

        public Pokedex()
        {
            string strLearnset;
            string strPokedex;
            using (StreamReader r = new StreamReader(PokeManager.thisNamespacePath + @"\JSONFiles\learnset.json"))
            {
                strLearnset = r.ReadToEnd();
            }
            using (StreamReader r = new StreamReader(PokeManager.thisNamespacePath + @"\JSONFiles\pokedex.json"))
            {
                strPokedex = r.ReadToEnd();
            }
            GetLearnsetsFromJSON(strLearnset);
            GetPokedexFromJSON(strPokedex);
        }

        //private async Task<Pokedex> InitializeAsync()
        //{
            //lsasyncData = await GetLearnsetsFromJSON(strLearnset);
            //pdasyncData = await GetPokedexFromJSON(strPokedex);
            //learnsets = await 
            //return this;
        //}


        //public static Task<Pokedex> CreateAsync()
        //{
            //var ret = new Pokedex();
            //return ret.InitializeAsync();
        //}

        //private async Task<string> ReadJSONFile(StreamReader rdr)
        //{
        //return await rdr.ReadToEndAsync();
        //}

        //FINISHED
        private async Task GetLearnsetsFromJSON(string jsfile)
        {
            //Dictionary<string, Learnset> learnsetsAsync = new Dictionary<string, Learnset>();
            SimpleJSON.JSONNode data = SimpleJSON.JSON.Parse(jsfile);
            List<string> learnsetKeys = new List<string>();
            List<Learnset> learnsetVals = new List<Learnset>();

            // CHECK THE SPECIES NAME
            foreach (KeyValuePair<string, SimpleJSON.JSONNode> kvp in data)
            {
                //string ke = kvp.Key.ToLower();
                string ke = FormatSpecies(kvp.Key);
                learnsetKeys.Add(ke);
            }
            int i = 0;

            // GET THE POKEMOVES
            foreach (SimpleJSON.JSONNode poke in data)
            {
                Learnset ls = new Learnset();
                List<string> moves = new List<string>();
                // CHECK THE MOVE NAME
                foreach (KeyValuePair<string, SimpleJSON.JSONNode> move in poke[0])
                {
                    bool gen3move = false;
                    // CHECK FOR VIABLE GEN AND METHOD
                    foreach (SimpleJSON.JSONNode movegen in move.Value)
                    {

                        if (movegen.ToString().Contains("3L") || movegen.ToString().Contains("3T") || movegen.ToString().Contains("3M") || movegen.ToString().Contains("3L"))
                        {
                            gen3move = true;
                            break;
                        }
                    }

                    if (gen3move)
                    {
                        moves.Add(move.Key);
                    }

                }
                moves = FormatMoves(moves);
                ls.AssignLearnset(learnsetKeys[i], moves);
                learnsetVals.Add(ls);
                i++;


            }
            foreach (Learnset l in learnsetVals)
            {
                
                learnsets.Add(l.species, l);
                //learnsetsAsync.Add(l.species, l);
                
                //Debug.Log(l.species + " moves: ");
                //foreach (string s in l.AllMoves)
                //Debug.Log(s + ", ");
            }

        }
        
        //FINISHED
        private async Task GetPokedexFromJSON(string jsfile)
        {
            SimpleJSON.JSONNode data = SimpleJSON.JSON.Parse(jsfile);
            List<Poke> pokeData = new List<Poke>();
            // CHECK THE SPECIES NAME
            foreach (SimpleJSON.JSONNode info in data)
            {
                Poke poData;
                int poNum = 0;
                int[] poStats = new int[6];
                int[] poEvs = new int[6] { 1, 0, 0, 0, 0, 0 };
                int[] poIvs = new int[6] { 31, 31, 31, 31, 31, 31 };
                string poSpecies, poAbil, poGender;
                string[] poTypes = new string[2];
                bool poSt = false;
                poAbil = "0";
                poGender = "";
                poSpecies = "";
                foreach (KeyValuePair<string, SimpleJSON.JSONNode> itype in info)
                {

                    // CHECK EACH 
                    switch (itype.Key)
                    {
                        case "num":
                            poNum = itype.Value;
                            break;
                        case "name":
                            poSpecies = itype.Value;
                            //poSpecies = poSpecies.ToLower();
                            poSpecies = FormatSpecies(poSpecies);
                            break;
                        case "types":
                            int i = 0;
                            foreach (SimpleJSON.JSONNode t in info["types"])
                            {
                                poTypes[i] = t.ToString();
                                i++;
                            }
                            break;
                        case "baseStats":
                            int j = 0;
                            foreach (SimpleJSON.JSONNode t in info["baseStats"])
                            {
                                poStats[j] = (int)t;
                                j++;
                            }
                            break;
                        case "abilities":
                            //TEMPORARY
                            poAbil = "0";
                            break;
                        case "tags":
                            poSt = true;
                            break;
                        default:

                            break;
                    }


                }
                if (poNum != 0)
                {


                    //string[] poMoves = new string[4];
                    //poMoves = RandomMoveset(learnsets[poSpecies]);
                    poData = new Poke(poNum, poSpecies, poSpecies, "silkscarf", poAbil, "", poTypes, poStats, poEvs, poGender, poIvs, poSt);

                    if (pokes.ContainsKey(poSpecies))
                    {
                        System.Diagnostics.Debug.WriteLine("CANT ADD ANOTHER " + poSpecies);
                    }
                    else
                    {
                        pokes.Add(poSpecies, poData);
                    }
                    if (pokeSpeciesByNum.ContainsKey(poNum))
                    {
                        System.Diagnostics.Debug.WriteLine("CANT ADD ANOTHER " + poSpecies);
                    }
                    else
                    {
                        pokeSpeciesByNum.Add(poNum, poSpecies);
                    }

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("TRIED TO CREATE MISSINGNO. BAD BAD BAD");
                }
            }
        }

        public Poke GetRandomPoke()
        {
            var rand = new Random();
            int r = rand.Next(377);
            Poke randompoke;
            while (bannedPokes.Contains(r) || !pokeSpeciesByNum.ContainsKey(r))
            {
                var newRand = new Random();
                r = newRand.Next(377);
            }
            if (!pokeSpeciesByNum.ContainsKey(r))
            {
                System.Diagnostics.Debug.WriteLine("THERE IS NO POKEMON WITH THE NUMBER " + r);
            }
            randompoke = pokes[pokeSpeciesByNum[r]];
            return randompoke;

        }

        public Poke GetRandomPokeOfType(string pType)
        {
            List<int> typeNums = new List<int>();
            //Debug.Log("PULLING LIST OF POKEMON TYPE " + pType);
            typeNums = pokeNumsByType[pType];
            string tests = "";
            tests += "HERES ALL THE POKEMON WITH THAT TYPE: ";
            foreach (int p in typeNums)
            {
                tests += pokeSpeciesByNum[p] + " ";
            }
            //Debug.Log(tests);
            var rand = new Random();
            int r = rand.Next(typeNums.Count);
            //Debug.Log("PICKING NUMBER " + r);
            Poke randompoke;
            int randPokeByNum = typeNums[r];
            //Debug.Log(randPokeType);
            while (bannedPokes.Contains(r))
            {
                //r = UnityEngine.Random.Range(0, typeNums.Count);
                var newRand = new Random();
                r = newRand.Next(typeNums.Count);
                randPokeByNum = typeNums[r];
            }
            randompoke = pokes[pokeSpeciesByNum[typeNums[r]]];
            System.Diagnostics.Debug.WriteLine("CHOSEN POKEMON WAS " + randompoke.Species);
            return randompoke;
        }

        public async Task CreatePokeTypeLists()
        {
            List<int> poketypeDark = new List<int>();
            List<int> poketypeNorm = new List<int>();
            List<int> poketypeFght = new List<int>();
            List<int> poketypeFly = new List<int>();
            List<int> poketypePois = new List<int>();
            List<int> poketypeGrnd = new List<int>();
            List<int> poketypeRock = new List<int>();
            List<int> poketypeBug = new List<int>();
            List<int> poketypeGhst = new List<int>();
            List<int> poketypeStel = new List<int>();
            List<int> poketypeFire = new List<int>();
            List<int> poketypeWatr = new List<int>();
            List<int> poketypeGras = new List<int>();
            List<int> poketypeElec = new List<int>();
            List<int> poketypePsyc = new List<int>();
            List<int> poketypeIce = new List<int>();
            List<int> poketypeDrag = new List<int>();
            foreach (KeyValuePair<string, Poke> kvp in pokes)
            {
                string tempt = kvp.Value.Types[0].ToLower();
                tempt = tempt.Trim('"');
                switch (tempt)
                {
                    case "dark":
                        poketypeDark.Add(kvp.Value.Num);
                        break;
                    case "normal":
                        poketypeNorm.Add(kvp.Value.Num);
                        break;
                    case "fighting":
                        poketypeFght.Add(kvp.Value.Num);
                        break;
                    case "flying":
                        poketypeFly.Add(kvp.Value.Num);
                        break;
                    case "poison":
                        poketypePois.Add(kvp.Value.Num);
                        break;
                    case "ground":
                        poketypeGrnd.Add(kvp.Value.Num);
                        break;
                    case "rock":
                        poketypeRock.Add(kvp.Value.Num);
                        break;
                    case "bug":
                        poketypeBug.Add(kvp.Value.Num);
                        break;
                    case "ghost":
                        poketypeGhst.Add(kvp.Value.Num);
                        break;
                    case "steel":
                        poketypeStel.Add(kvp.Value.Num);
                        break;
                    case "fire":
                        poketypeFire.Add(kvp.Value.Num);
                        break;
                    case "water":
                        poketypeWatr.Add(kvp.Value.Num);
                        break;
                    case "grass":
                        poketypeGras.Add(kvp.Value.Num);
                        break;
                    case "electric":
                        poketypeElec.Add(kvp.Value.Num);
                        break;
                    case "psychic":
                        poketypePsyc.Add(kvp.Value.Num);
                        break;
                    case "ice":
                        poketypeIce.Add(kvp.Value.Num);
                        break;
                    case "dragon":
                        poketypeDrag.Add(kvp.Value.Num);
                        break;

                    default:
                        //Debug.Log("This pokemon has a messed up type: " + kvp.Key + " - " + tempt + "-" + tempt.Length + " chars");
                        break;
                }
            }
            pokeNumsByType.Add("dark", poketypeDark);
            pokeNumsByType.Add("normal", poketypeNorm);
            pokeNumsByType.Add("fighting", poketypeFght);
            pokeNumsByType.Add("flying", poketypeFly);
            pokeNumsByType.Add("poison", poketypePois);
            pokeNumsByType.Add("ground", poketypeGrnd);
            pokeNumsByType.Add("rock", poketypeRock);
            pokeNumsByType.Add("bug", poketypeBug);
            pokeNumsByType.Add("ghost", poketypeGhst);
            pokeNumsByType.Add("steel", poketypeStel);
            pokeNumsByType.Add("fire", poketypeFire);
            pokeNumsByType.Add("water", poketypeWatr);
            pokeNumsByType.Add("grass", poketypeGras);
            pokeNumsByType.Add("electric", poketypeElec);
            pokeNumsByType.Add("psychic", poketypePsyc);
            pokeNumsByType.Add("ice", poketypeIce);
            pokeNumsByType.Add("dragon", poketypeDrag);
        }

        private string[] RandomMoveset(Learnset ls)
        {
            string[] ms = new string[4] { "", "", "", "" };
            int full = 0;
            while (full <= ls.AllMoves.Count && full < 4)
            {
                //int r = UnityEngine.Random.Range(0, ls.AllMoves.Count);
                var rand = new Random();
                int r = rand.Next(ls.AllMoves.Count);
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
            return ms;
        }

        public string[] FormatMoves(string[] mvs)
        {
            string[] formatted = new string[mvs.Length];
            int i = 0;
            foreach (string m in mvs)
            {
                string formString = "";
                foreach (char c in m)
                {
                    if (System.Char.IsLetter(c))
                    {
                        formString += System.Char.ToLower(c);
                    }
                }
                formatted[i] = formString;
                i++;
            }
            return formatted;
        }

        public List<string> FormatMoves(List<string> mvs)
        {
            List<string> formatted = new List<string>();
            foreach (string m in mvs)
            {
                string formString = "";
                foreach (char c in m)
                {
                    if (System.Char.IsLetter(c))
                    {
                        formString += System.Char.ToLower(c);
                    }
                }
                formatted.Add(formString);
            }
            return formatted;
        }

        public string FormatSpecies(string spec)
        {
            string fspec = "";
            int i = 0;
            foreach (char ch in spec)
            {
                char fchar = ch;
                //CAP FIRST CHAR
                if (i == 0)
                {
                    fchar = System.Char.ToUpper(fchar);

                }
                //FOR STUPID LITTLE FUCKING MEN LIKE Mr. Mime AND Nidoran-M
                else if (fspec.Length > 1 && (fspec.Substring(fspec.Length - 1) == " " || fspec.Substring(fspec.Length - 1) == "-"))
                {
                    fchar = System.Char.ToUpper(fchar);
                }
                //OTHER LETTERS LOWERCASE
                else if (System.Char.IsLetter(ch))
                {
                    fchar = System.Char.ToLower(ch);
                }

                else
                {
                    fchar = ch;
                }
                fspec += fchar;
                i++;
            }
            return fspec;
        }

        //public Learnset GetLearnsetFromSpecies(string spec)
        //{
        //    return learnsets[spec];
        //}

        //public Poke GetPokeFromSpecies(string spec)
        //{
        //    return pokes[spec];
        //}

        //public string GetPokeSpeciesFromNum(int pnum)
        //{
        //    return pokeSpeciesByNum[pnum];
        //}

    }
}
