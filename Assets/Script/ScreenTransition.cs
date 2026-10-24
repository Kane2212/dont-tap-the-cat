using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenTransition : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 0.5f;

    private static ScreenTransition instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0;
            fadeImage.color = color;
        }
    }

    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(FadeTransition(sceneName));
    }

    private IEnumerator FadeTransition(string sceneName)
    {
        yield return FadeIn();
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        yield return FadeOut();
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsed / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        color.a = 1;
        fadeImage.color = color;
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0;
        Color color = fadeImage.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Clamp01(1 - (elapsed / fadeDuration));
            fadeImage.color = color;
            yield return null;
        }

        color.a = 0;
        fadeImage.color = color;
    }

    public static ScreenTransition GetInstance()
    {
        return instance;
    }
}
