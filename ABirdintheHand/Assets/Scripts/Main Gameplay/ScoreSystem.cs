using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreSystem : MonoBehaviour
{
    public static float CurrentScore = 0;
    public int winningScore = 10;

    [SerializeField] TextMeshProUGUI scoreAmount;

    private void start()
    {
        CurrentScore = 0;
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