using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <Summary>
/// Scene management for bird win condition & Scene Buttons
/// </Summary>

public class SceneMenuManager : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Score required for birds to win")]
    [SerializeField] private int birdWinningScore = 30;

    [Header("Scene Names")]
    [SerializeField] private string birdsWinScene = "BirdsWin";
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string mainLevelScene = "MainLevel";

    [Header("Score System")]
    [SerializeField] private ScoreSystem scoreSystem;

    private bool gameEnded = false;

    private void Start()
    {
        if (scoreSystem == null)
        {
            scoreSystem = FindObjectOfType<ScoreSystem>();
        }
    }

    private void Update()
    {
        if (!gameEnded && scoreSystem != null && scoreSystem.GetScore() >= birdWinningScore)
        {
            WinGame();
        }
    }

    #region Scene Buttons

    public void StartGame(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void BackToLobby()
    {
        SceneManager.LoadScene(mainLevelScene);
    }

    public void GoToTitleScreen()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    private void WinGame()
    {
        if (!gameEnded)
        {
            gameEnded = true;
            SceneManager.LoadScene(birdsWinScene);
        }
    }

    #endregion
}
