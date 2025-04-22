using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdWin : MonoBehaviour
{
    public int winningScore = 5;
    public ScoreSystem scoreSystem;
    private bool gameEnded;

    void Start()
    {
        scoreSystem = FindObjectOfType<ScoreSystem>();
    }

    void Update()
    {
        if (scoreSystem != null && scoreSystem.GetScore() >= winningScore)
        {
            WinGame();
        }
    }

    void WinGame()
    {
        if (!gameEnded)
        {

            Debug.Log("You Win!");
            gameEnded = true;
        }
    }
}
