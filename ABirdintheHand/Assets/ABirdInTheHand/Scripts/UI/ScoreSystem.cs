using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Central score system
/// </summary>
public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    [Header("Game Settings")]
    [SerializeField] private int winningScore = 10;

    [Header("UI References")]
    [SerializeField] private GameObject scoreUIRoot;
    [SerializeField] private TextMeshProUGUI scoreAmount;

    private float currentScore = 0f;

    public static event Action<float> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[ScoreSystem] Duplicate instance detected, destroying", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (scoreAmount == null)
        {
            scoreAmount = FindObjectOfType<TextMeshProUGUI>();

            if (scoreAmount != null)
            {
                Debug.Log($"[ScoreSystem] Auto-assigned scoreAmount to {scoreAmount.name}", this);
            }
            else
            {
                Debug.LogWarning("[ScoreSystem] scoreAmount not assigned and no TMP found. UI won't update.", this);
            }
        }

        if (scoreUIRoot != null)
        {
            scoreUIRoot.SetActive(false);
        }

        SetScore(0f);
    }

    public void AddScore(float amount)
    {
        if (amount <= 0f)
        {
            Debug.LogWarning($"[ScoreSystem] Attempted to add non-positive score: {amount}", this);
            return;
        }

        currentScore += amount;
        PublishScore();

        Debug.Log($"[ScoreSystem] Added {amount} points. New score: {currentScore}", this);
    }

    public void SetScore(float value)
    {
        if (value < 0f)
        {
            Debug.LogWarning($"[ScoreSystem] Attempted to set negative score: {value}", this);
            value = 0f;
        }

        currentScore = value;
        PublishScore();
    }

    public float GetScore()
    {
        return currentScore;
    }

    public int GetWinningScore()
    {
        return winningScore;
    }

    public void SetScoreUIVisible(bool visible)
    {
        if (scoreUIRoot != null)
        {
            scoreUIRoot.SetActive(visible);
        }
    }

    private void PublishScore()
    {
        if (scoreAmount != null)
        {
            scoreAmount.text = currentScore.ToString("0");
        }

        OnScoreChanged?.Invoke(currentScore);
    }

    public void ResetScore()
    {
        SetScore(0f);
        Debug.Log("[ScoreSystem] Score reset to 0", this);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}