using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanLose : MonoBehaviour
{
    public int losingScore = 5;
    public ScoreSystem scoreSystem;
    private bool gameEnded;

    void Start()
    {
        scoreSystem = FindObjectOfType<ScoreSystem>();
    }

    void Update()
    {
        if (scoreSystem != null && scoreSystem.GetScore() >= losingScore)
        {
            LoseGame();
        }
    }

    void LoseGame()
    {
        if (!gameEnded)
        {

            Debug.Log("You Lose!");
            gameEnded = true;
        }
    }
}