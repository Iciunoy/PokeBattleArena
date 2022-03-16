using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class GameStates
{
    
    public GameStates()
    {

    }
    public enum GameState
    {
        starting,
        exiting,
        teamgen,
        betting,
        battle,
        results,
        reloading,
        clearing,
        errorstate
    }
    public string GameStateToString(GameState gs)
    {
        string gstring = "errorstate";
        switch (gs)
        {
            case GameState.starting:
                gstring = "starting";
                break;
            case GameState.exiting:
                gstring = "exiting";
                break;
            case GameState.teamgen:
                gstring = "teamgen";
                break;
            case GameState.betting:
                gstring = "betting";
                break;
            case GameState.battle:
                gstring = "battle";
                break;
            case GameState.results:
                gstring = "results";
                break;
            case GameState.reloading:
                gstring = "reloading";
                break;
            case GameState.clearing:
                gstring = "clearing";
                break;
            default:
                break;
        }
        return gstring;
    }
}
