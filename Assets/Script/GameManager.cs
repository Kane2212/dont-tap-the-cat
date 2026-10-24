using NUnit.Framework;
using UnityEngine;

// Scope Tanggung jawab
// 1. Menyimpan final score, high score, dan menyediakan agar bisa diakses oleh result scene
// 2. Menyediakan konfigurasi permainan yang diperlukan akses antar scene
// 3. Memastikan bahwa data permanen tetap tersimpan meskipun aplikasi ditutup
// 4. Harus ada 1 saja di setiap scene

public class GameManager : MonoBehaviour
{

    private int highScore;

    private int volumeConfig;
    private bool soundOn = true;


    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        volumeConfig = PlayerPrefs.GetInt("VolumeConfig", 100);
        soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
    }

    public void SaveFinalScore(int score)
    {
        if(score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }


    public int GetHighScore()
    {
        return highScore;
    }

    public int GetVolumeConfig()
    {
        return volumeConfig;
    }

    public int GetSoundOn()
    {
        return soundOn ? 1 : 0;
    }
}
