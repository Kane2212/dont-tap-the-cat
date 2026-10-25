using UnityEngine;

public class GamePlayUIHandler : MonoBehaviour
{
    public GameObject gameStartPanel;
    public GameObject pausePanel;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    public void Gamestart()
    {
        gameStartPanel.SetActive(false);
        GamePlayManager.instance.OnGameStart();
    }


    public void Pause_button()
    {
        pausePanel.SetActive(true);
        GamePlayManager.instance.OnPause();

    }

    public void Resume_button()
    {
        pausePanel.SetActive(false);
        GamePlayManager.instance.OnResume();
    }
    
}
