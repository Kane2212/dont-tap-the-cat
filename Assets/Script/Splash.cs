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
        ScreenTransition.GetInstance().TransitionToScene("MainMenu");
        hasTransitioned = true;
    }
}