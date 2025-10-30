using System;
using UnityEngine;
using TMPro;

/// <summary>
/// Central score system
/// </summary>

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance { get; private set; }

    public static float CurrentScoreInternal = 0f;

    public int winningScore = 10;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI scoreAmount;

    public static event Action<float> OnScoreChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[ScoreSystem] Another instance exists — destroying this one.");
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
            TextMeshProUGUI found = GameObject.FindObjectOfType<TextMeshProUGUI>();
            if (found != null)
            {
                scoreAmount = found;
                Debug.Log($"[ScoreSystem] Auto-assigned scoreAmount to {found.name}");
            }
            else
            {
                Debug.LogWarning("[ScoreSystem] scoreAmount not assigned and no TMP found in scene. UI won't update.");
            }
        }
        CurrentScoreInternal = 0f;
        PublishScore();
    }

    public void AddScore(float amount)
    {
        CurrentScoreInternal += amount;
        PublishScore();
    }

    public void SetScore(float value)
    {
        CurrentScoreInternal = value;
        PublishScore();
    }

    public float GetScore() => CurrentScoreInternal;

    private void PublishScore()
    {
        if (scoreAmount != null)
            scoreAmount.text = CurrentScoreInternal.ToString("0");

        OnScoreChanged?.Invoke(CurrentScoreInternal);
    }
}
