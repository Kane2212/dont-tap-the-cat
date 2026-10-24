using UnityEngine;
using TMPro;

public class ScoreHandler : MonoBehaviour
{
    private int score;
    public TextMeshProUGUI scoreText;

    void Start()
    {
        score = 0;
        UpdateScoreDisplay();
    }


    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreDisplay();
    }

    public int GetScore()
    {
        return score;
    }
    
    public void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }
}
