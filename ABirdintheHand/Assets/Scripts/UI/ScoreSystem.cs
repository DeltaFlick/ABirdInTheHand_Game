using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreSystem : MonoBehaviour
{
    public static float CurrentScore = 0;
    public int winningScore = 10;

    [SerializeField] TextMeshProUGUI scoreAmount;

    private void Start()
    {
        CurrentScore = 0;

        if (scoreAmount != null && scoreAmount.transform.parent != null)
        {
            scoreAmount.transform.parent.gameObject.SetActive(false);
        }

        UpdateScoreUI();
    }

    public void AddScore(float amount)
    {
        CurrentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        scoreAmount.text = CurrentScore.ToString("0");
    }

    public float GetScore()
    {
        return CurrentScore;
    }
}
