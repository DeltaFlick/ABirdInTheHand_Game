using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BirdWin : MonoBehaviour
{
    public int winningScore = 30;
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

            SceneManager.LoadScene("BirdsWin");
            gameEnded = true;
        }
    }
}
