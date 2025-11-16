using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages scene transitions and win conditions
/// </summary>
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
    private float lastScore = 0f;

    private void Awake()
    {
        if (scoreSystem == null)
        {
            scoreSystem = FindObjectOfType<ScoreSystem>();
        }

        if (scoreSystem == null)
        {
            Debug.LogWarning("[SceneMenuManager] ScoreSystem not found - win condition disabled", this);
        }
    }

    private void Start()
    {
        ScoreSystem.OnScoreChanged += OnScoreChanged;
    }

    private void OnDestroy()
    {
        ScoreSystem.OnScoreChanged -= OnScoreChanged;
    }

    private void OnScoreChanged(float newScore)
    {
        lastScore = newScore;
        CheckWinCondition();
    }

    private void Update()
    {
        if (!gameEnded && scoreSystem != null)
        {
            float currentScore = scoreSystem.GetScore();

            if (currentScore != lastScore)
            {
                lastScore = currentScore;
                CheckWinCondition();
            }
        }
    }

    private void CheckWinCondition()
    {
        if (!gameEnded && lastScore >= birdWinningScore)
        {
            WinGame();
        }
    }

    #region Scene Buttons

    public void StartGame(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"[SceneMenuManager] Invalid scene index: {sceneIndex}", this);
            return;
        }

        Debug.Log($"[SceneMenuManager] Loading scene index: {sceneIndex}", this);
        SceneManager.LoadScene(sceneIndex);
    }

    public void BackToLobby()
    {
        if (string.IsNullOrEmpty(mainLevelScene))
        {
            Debug.LogError("[SceneMenuManager] Main level scene name not set!", this);
            return;
        }

        Debug.Log($"[SceneMenuManager] Loading lobby: {mainLevelScene}", this);
        SceneManager.LoadScene(mainLevelScene);
    }

    public void GoToTitleScreen()
    {
        if (string.IsNullOrEmpty(mainMenuScene))
        {
            Debug.LogError("[SceneMenuManager] Main menu scene name not set!", this);
            return;
        }

        Debug.Log($"[SceneMenuManager] Loading main menu: {mainMenuScene}", this);
        SceneManager.LoadScene(mainMenuScene);
    }

    public void QuitGame()
    {
        Debug.Log("[SceneMenuManager] Quitting game", this);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void WinGame()
    {
        if (gameEnded) return;

        if (string.IsNullOrEmpty(birdsWinScene))
        {
            Debug.LogError("[SceneMenuManager] Birds win scene name not set!", this);
            return;
        }

        gameEnded = true;
        Debug.Log($"[SceneMenuManager] Birds win! Loading scene: {birdsWinScene}", this);
        SceneManager.LoadScene(birdsWinScene);
    }

    #endregion
}