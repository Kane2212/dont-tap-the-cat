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



    // LeaderBoard Display


    [Header("LeaderBoard")]
    public GameObject leaderBoardPanel;
    public GameObject textContainer;
    public TextMeshProUGUI[] textList;


    
    private bool isAudioOn = true;

    void Start()
    {

        playButton.onClick.AddListener(PlayGame);
        settingsButton.onClick.AddListener(OpenSettings);
        audioButton.onClick.AddListener(ToggleAudio);


        textList = textContainer.GetComponentsInChildren<TextMeshProUGUI>();

        UpdateAudioDisplay();
        SoundManager.GetInstance().Play("MainMenu");



    }

    public void PlayGame()
    {
        ScreenTransition.GetInstance().TransitionToScene("Game");
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void ToggleAudio()
    {
        isAudioOn = !isAudioOn;
        UpdateAudioDisplay();

        if (isAudioOn)
        {
            SoundManager.GetInstance().Mute(isAudioOn);
        }
        else
        {
            SoundManager.GetInstance().Mute(!isAudioOn);
        }
    }
    

    private void UpdateAudioDisplay()
    {
        if (audioIcon != null)
        {
            audioIcon.sprite = isAudioOn ? audioOnSprite : audioOffSprite;
        }
    }

    public void OpenLeaderBoard()
    {
        leaderBoardPanel.SetActive(true);
    }

    public void CloseLeaderBoardPanel()
    {
        leaderBoardPanel.SetActive(false);
    }
}
