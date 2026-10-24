using UnityEngine;

// Script ini bertugas untuk membuat setup sebelum GamePlay disuguhkan pada player
// Meliputi :
// - Timer
// - Score
// - Lokasi
//
// Scipt ini juga bertugas untuk mengatur jalannya flow permainan, sehingga dia adalah pusat untuk keputusan Game State

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
        SoundManager.GetInstance().Play("GamePlay");
    }

    private void Update()
    {
        
    }

    public GameState GetState()
    {
        return state;
    }
}
