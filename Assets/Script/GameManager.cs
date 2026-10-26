using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int highScore;
    private int volumeConfig;
    private bool soundOn = true;
    private int finalScore;

    public static GameManager Instance { get; private set; }

    public int FinalScore
    {
        get { return finalScore; }
    }

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
            return;
        }

        LoadSettings();
    }

    private void Start()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        volumeConfig = PlayerPrefs.GetInt("VolumeConfig", 100);
        soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;
    }

    public void SaveFinalScore(int score)
    {
        finalScore = score;
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
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

    public void SetSoundOn(bool enabled)
    {
        soundOn = enabled;
        PlayerPrefs.SetInt("SoundOn", soundOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetVolumeConfig(int value)
    {
        volumeConfig = Mathf.Clamp(value, 0, 100);
        PlayerPrefs.SetInt("VolumeConfig", volumeConfig);
        PlayerPrefs.Save();
    }

    public int GetFinalScore()
    {
        return finalScore;
    }
}
