using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

/// <Summary>
/// Timer system for game rounds
/// </Summary>

public class Timer : MonoBehaviour
{
    public RoundStart roundStart;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float remainingTime;
    private bool timerRunning = false;

    [Header("Flashing Settings")]
    [SerializeField] private float flashThreshold = 10f;
    [SerializeField] private float flashSpeed = 4f;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private Color normalColor = Color.white;

    void Start()
    {
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
            timerText.color = normalColor;
            timerText.transform.localScale = Vector3.one;
        }
    }

    public void ResetTimer(float timeInSeconds)
    {
        remainingTime = timeInSeconds;
        timerRunning = true;

        if (timerText != null)
        {
            timerText.color = normalColor;
            timerText.transform.localScale = Vector3.one;
            timerText.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (!timerRunning) return;

        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("{0}:{1:00}", minutes, seconds);

            if (remainingTime <= flashThreshold)
            {
                float lerp = Mathf.PingPong(Time.time * flashSpeed, 1f);
                timerText.color = Color.Lerp(normalColor, flashColor, lerp);

                float pulse = 1f + Mathf.PingPong(Time.time * pulseSpeed, 0.2f);
                timerText.transform.localScale = new Vector3(pulse, pulse, pulse);
            }
        }
        else
        {
            remainingTime = 0;
            timerRunning = false;

            timerText.color = flashColor;
            timerText.transform.localScale = Vector3.one;
            timerText.text = "00:00";

            if (roundStart != null)
            {
                roundStart.EndRound();
            }

            SceneManager.LoadScene("Draw");
        }
    }
}
