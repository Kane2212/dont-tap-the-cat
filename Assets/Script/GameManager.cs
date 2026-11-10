using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private int highScore;
    private int volumeConfig;
    private bool soundOn = true;
    private bool sfxOn = true;
    private bool musicOn = true;
    private bool notificationsOn = true;
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

    }

    private void LoadSettings()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        volumeConfig = PlayerPrefs.GetInt("VolumeConfig", 100);
        if(PlayerPrefs.HasKey("SoundOn"))
        {
            soundOn = PlayerPrefs.GetInt("SoundOn") == 0 ? true : false;
        }
        else
        {
            soundOn = true; // Default value if the key doesn't exist
        }

        sfxOn = PlayerPrefs.GetInt("SFXOn", 0) == 0;
        musicOn = PlayerPrefs.GetInt("MusicOn", 0) == 0;
        notificationsOn = PlayerPrefs.GetInt("NotificationsOn", 0) == 0;
    }

    public void SaveFinalScore(int score)
    {
        finalScore = score;
        List<int> highScores = GetHighScores();
        for (int i = 0; i < highScores.Count; i++)
        {
            if (score > highScores[i])
            {
                highScores.Insert(i, score);
                PlayerPrefs.SetInt($"HighScore_{i}", score);
                PlayerPrefs.Save();
                break;
            }
        }
        
        if (finalScore > highScore)
        {
            highScore = finalScore;
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

    public bool GetSoundOn()
    {
        return soundOn;
    }

    public void SetSoundOn(bool enabled)
    {
        soundOn = enabled;
        PlayerPrefs.SetInt("SoundOn", soundOn ? 0 : 1);
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

    internal void SetNotificationsOn(bool isNotificationsOn)
    {
        PlayerPrefs.SetInt("NotificationsOn", isNotificationsOn ? 0 : 1);
        notificationsOn = isNotificationsOn;
        PlayerPrefs.Save();
    }

    internal void SetSFXOn(bool isSFXOn)
    {
        PlayerPrefs.SetInt("SFXOn", isSFXOn ? 0 : 1);
        sfxOn = isSFXOn;
        PlayerPrefs.Save();
    }

    internal void SetMusicOn(bool isMusicOn)
    {
        PlayerPrefs.SetInt("MusicOn", isMusicOn ? 0 : 1);
        musicOn = isMusicOn;
        PlayerPrefs.Save();
    }

    internal bool GetSFXOn()
    {
        return sfxOn;
    }

    internal bool GetMusicOn()
    {
        return musicOn;
    }

    internal bool GetNotificationsOn()
    {
        return notificationsOn;
    }

    public List<int> GetHighScores()
    {
        List<int> highScores = new List<int>();
        for (int i = 0; i < 10; i++)
        {
            int score = PlayerPrefs.GetInt($"HighScore_{i}", 0);
            highScores.Add(score);
        }
        return highScores;
    }
}
