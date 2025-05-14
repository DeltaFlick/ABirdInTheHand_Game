using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    public RoundStart roundStart;

    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] float remainingTime;

    void Start()
    {
        if (timerText != null)
        {
            timerText.gameObject.SetActive(false);
        }
    }

    public void ResetTimer(float timeInSeconds)
    {
        remainingTime = timeInSeconds;
        timerText.color = Color.white;

        if (timerText != null)
        {
            timerText.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (roundStart != null && roundStart.startRoundActive)
        {
            StartTimer();
        }
    }

    public void StartTimer()
    {
        if (remainingTime > 0)
        {
            remainingTime -= Time.deltaTime;
        }
        else
        {
            remainingTime = 0;
            timerText.color = Color.red;

            if (roundStart != null)
            {
                roundStart.EndRound();
            }

            SceneManager.LoadScene("Draw");
        }

        int minutes = Mathf.FloorToInt(remainingTime / 60);
        int seconds = Mathf.FloorToInt(remainingTime % 60);
        timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
    }
}
