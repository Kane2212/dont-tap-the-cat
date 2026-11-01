using UnityEngine;
using UnityEngine.SceneManagement;

public class Splash : MonoBehaviour
{
    [SerializeField] private float splashDuration = 3f;
    private float timer;
    bool hasTransitioned = false;
    void Start()
    {
        timer = 0;
        SoundManager.GetInstance().Play("Splash");
        SoundManager.GetInstance().Mute(GameManager.Instance.GetSoundOn() ? false : true);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= splashDuration)
        {
            if(hasTransitioned) return;
            LoadMainMenu();
        }

        if (Input.anyKeyDown)
        {
            if(hasTransitioned) return;
            LoadMainMenu();
        }
    }

    private void LoadMainMenu()
    {
        SoundManager.GetInstance().Stop("Splash");
        ScreenTransition.GetInstance().TransitionToScene("MainMenu");
        hasTransitioned = true;
    }
}