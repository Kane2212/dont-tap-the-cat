using System;
using System.Collections.Generic;
using UnityEngine;


public enum GameState
{
    Ready,
    Playing,
    Finished,
    Paused
}

public class GamePlayManager : MonoBehaviour
{

    

    private GameState state;

    public static GamePlayManager instance;


    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
    }

    public void OnGameStart()
    {
        SoundManager.GetInstance().Play("GamePlay");
    }

    private void Update()
    {
        
    }

    public GameState GetState()
    {
        return state;
    }

    public void OnPause()
    {

    }

    public void OnResume()
    {

    }

}
