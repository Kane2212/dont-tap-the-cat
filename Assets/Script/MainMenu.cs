using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button audioButton;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Image audioIcon;
    [SerializeField] private Sprite audioOnSprite;
    [SerializeField] private Sprite audioOffSprite;


    [Header("Settings")]
    [SerializeField] private Sprite OnSprite;
    [SerializeField] private Sprite OffSprite;
    [SerializeField] private Image SFXSprite;
    [SerializeField] private Image musicSprite;
    [SerializeField] private Image notificationSprite;


    // LeaderBoard Display


    [Header("LeaderBoard")]
    public GameObject leaderBoardPanel;
    public GameObject textContainer;
    public TextMeshProUGUI[] textList;


    
    private bool isAudioOn = true;
    private bool isSFXOn = true;
    private bool isMusicOn = true;
    private bool isNotificationsOn = true;

    private string language = "ind"; // Default language

    void Start()
    {

        playButton.onClick.AddListener(PlayGame);
        settingsButton.onClick.AddListener(OpenSettings);
        audioButton.onClick.AddListener(ToggleAudio);


        textList = textContainer.GetComponentsInChildren<TextMeshProUGUI>();
        isAudioOn = GameManager.Instance.GetSoundOn();
        isSFXOn = GameManager.Instance.GetSFXOn();
        isMusicOn = GameManager.Instance.GetMusicOn();
        isNotificationsOn = GameManager.Instance.GetNotificationsOn();
        UpdateAudioDisplay();
        SoundManager.GetInstance().Play("MainMenu");

    }

    public void PlayGame()
    {
        ScreenTransition.GetInstance().TransitionToScene("Game");
        SoundManager.GetInstance().Play("UI_Button");
        SoundManager.GetInstance().Stop("MainMenu");
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            SoundManager.GetInstance().Play("UI_Button");
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            SoundManager.GetInstance().Play("UI_BackButton");
        }
    }

    public void ToggleAudio()
    {

        if (isAudioOn)
        {
            isAudioOn = false;
            GameManager.Instance.SetSoundOn(false);
            SoundManager.GetInstance().Mute(true);
        }
        else
        {
            isAudioOn = true;
            GameManager.Instance.SetSoundOn(true);
            SoundManager.GetInstance().Mute(false);
            SoundManager.GetInstance().Play("UI_Button");
        }
        UpdateAudioDisplay();
        UpdateLeaderBoard();
    }
    

    private void UpdateAudioDisplay()
    {
        if (audioIcon != null)
        {
            audioIcon.sprite = isAudioOn ? audioOnSprite : audioOffSprite;
        }
    }

    public void UpdateLeaderBoard()
    {
        List<int> highScores = GameManager.Instance.GetHighScores();
        for (int i = 0; i < textList.Length; i++)
        {
            if (i < highScores.Count)
            {
                textList[i].text = $"{i + 1}. {highScores[i]}";
            }
            else
            {
                textList[i].text = $"{i + 1}. ---";
            }
        }
    }

    public void OpenLeaderBoard()
    {
        leaderBoardPanel.SetActive(true);
        SoundManager.GetInstance().Play("UI_Button");
    }

    public void CloseLeaderBoardPanel()
    {
        leaderBoardPanel.SetActive(false);
        SoundManager.GetInstance().Play("UI_BackButton");
    }

    //Settings Panel Button Functions
    public void ToggleSFX()
    {
        isSFXOn = isSFXOn ? false : true;
        SoundManager.GetInstance().MuteSFX(isSFXOn);
        GameManager.Instance.SetSFXOn(isSFXOn);
        UpdateSettingsDisplay();
    }
    public void ToggleMusic()
    {
        isMusicOn = isMusicOn ? false : true;
        SoundManager.GetInstance().MuteMusic(isMusicOn);
        GameManager.Instance.SetMusicOn(isMusicOn);
        UpdateSettingsDisplay();
    }
    public void ToggleNotifications()
    {
        isNotificationsOn = isNotificationsOn ? false : true;
        GameManager.Instance.SetNotificationsOn(isNotificationsOn);
        UpdateSettingsDisplay();
    }
    public void UpdateSettingsDisplay()
    {
        SFXSprite.sprite = isSFXOn ? OnSprite : OffSprite;
        musicSprite.sprite = isMusicOn ? OnSprite : OffSprite;
        notificationSprite.sprite = isNotificationsOn ? OnSprite : OffSprite;
    }

}
