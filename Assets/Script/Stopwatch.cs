using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Stopwatch : MonoBehaviour
{

    private float elapsedTime;
    private bool isRunning;
    private int lastMinutePassed;

    public UnityEvent<int> onMinutePassed = new UnityEvent<int>();


    public TextMeshProUGUI timeText;

    void Start()
    {
        elapsedTime = 0f;
        isRunning = false;
        lastMinutePassed = 0;
    }

    void Update()
    {
        if (GamePlayManager.instance.GetState() != GameState.Playing)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        int minutesPassed = Mathf.FloorToInt(elapsedTime / 60f);
        if (minutesPassed > lastMinutePassed)
        {
            lastMinutePassed = minutesPassed;
            onMinutePassed.Invoke(minutesPassed);
        }
        UpdateTimeText();
    }

    public void StartTimer()
    {
        isRunning = true;
    }

    public void StopTimer()
    {
        isRunning = false;
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        isRunning = false;
        lastMinutePassed = 0;
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    public string GetTimeString()
    {
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void UpdateTimeText()
    {
        if (timeText != null)
        {
            timeText.text = GetTimeString();
        }
    }

}
