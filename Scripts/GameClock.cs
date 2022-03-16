using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

public class GameClock : MonoBehaviour
{
    public static GameClock Instance { get; private set; }
    private Dictionary<float, UnityEvent> timers = new Dictionary<float, UnityEvent>();
    private float nextTimer;
    public bool isPaused { get; private set; }
    /// <summary>
    /// Time since the current pause began in seconds
    /// </summary>
    public float CurrentPausedTime { get; private set; }
    /// <summary>
    /// Time since the start of the game in seconds, ignoring paused time
    /// </summary>
    public float TotalUnpausedTime { get; private set; }

    private void Awake()
    {
        Instance = this;
        isPaused = false;
    }

    void Start()
    {
        StartCoroutine("UnpausedClock");
    }
    
    // Update is called once per frame
    void Update()
    {
        //Is the clock paused?
        if (isPaused)
        {
            //Unpause the clock
            if (!GameManager.Instance.isGamePaused)
            {
                isPaused = false;
                UnityEngine.Debug.Log("UNPAUSING GAME CLOCK");
                return;
            }
        }
        else
        {
            //It should be paused
            if (GameManager.Instance.isGamePaused)
            {
                isPaused = true;
                UnityEngine.Debug.Log("PAUSING GAME CLOCK");
                return;
            }

            //Check timers
            if (timers.Count > 0 && nextTimer != timers.Keys.Min())
            {
                nextTimer = timers.Keys.Min();
            }
            if (timers.Count > 0 && TotalUnpausedTime > nextTimer)
            {
                if (timers.ContainsKey(nextTimer))
                {
                    RemoveTimer(nextTimer);
                }
            }
        }

    }

    //FUNCTIONS

    /// <summary>
    /// Creates Timer. After n seconds, invoke event.
    /// </summary>
    /// <param name="seconds"></param>
    /// <param name="ev"></param>
    public void CreateTimer(float seconds, UnityEvent ev, bool isunique)
    {
        float finishTime = TotalUnpausedTime + seconds;
        if (isunique)
        {
            foreach (KeyValuePair<float, UnityEvent> kvp in timers)
            {
                float k = kvp.Key;
                UnityEvent e = kvp.Value;
                if (e == ev)
                {
                    timers.Remove(k);
                    timers.Add(finishTime, ev);
                    return;
                }
            }
        }
        
        while (timers.ContainsKey(finishTime))
        {
            finishTime += 0.1f;
        }
        timers.Add(finishTime, ev);
    }

    /// <summary>
    /// Cancels the next timer with matching event
    /// </summary>
    /// <param name="ev"></param>
    public void CancelNextTimerByEvent(UnityEvent ev)
    {
        float m = 0;
        foreach (KeyValuePair<float, UnityEvent> kvp in timers)
        {
            if (kvp.Value == ev)
            { 
                if (m == 0f || m > kvp.Key)
                {
                    m = kvp.Key;
                }
            }
        }
        timers.Remove(m);
    }
    
    /// <summary>
    /// Invokes timer's event, then removes timer
    /// </summary>
    /// <param name="minKey"></param>
    void RemoveTimer(float minKey)
    {
        timers[minKey].Invoke();
        timers.Remove(minKey);
    }

    /// <summary>
    /// Waits for a second, then updates the timer. Only called when the game is not paused.
    /// </summary>
    /// <returns></returns>
    IEnumerator UnpausedClock()
    {
        yield return new WaitForSeconds(1);
        
        if (isPaused)
        {
            CurrentPausedTime = 0;
            StartCoroutine("PausedClock");
        }
        else
        {
            //UnityEngine.Debug.Log("TICK");
            TotalUnpausedTime++;
            StartCoroutine("UnpausedClock");
        }
    }
    IEnumerator PausedClock()
    {
        yield return new WaitForSeconds(1);
        if (isPaused)
        {
            //UnityEngine.Debug.Log("TOCK");
            CurrentPausedTime++;
            StartCoroutine("PausedClock");
        }
        else
        {
            //UnityEngine.Debug.Log("TICK");
            TotalUnpausedTime++;
            StartCoroutine("UnpausedClock");
        }
    }

}
