using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class RoundStart : MonoBehaviour
{
    [Header("Teleport Settings (one per player, ordered by index)")]
    public GameObject[] teleportDestinations;

    [Header("Timer Reference")]
    public Timer timer;

    [Header("Round Control")]
    public int totalPlayers;
    private int playersInTrigger = 0;
    public bool startRoundActive = false;
    private Coroutine countdownCoroutine;
    private HashSet<GameObject> teleportedPlayers = new HashSet<GameObject>();

    [Header("UI Elements")]
    public GameObject scoreTextObject;
    public GameObject timerTextObject;
    public TextMeshProUGUI countdownText;

    [Header("Countdown Settings")]
    public float countdownDuration = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInTrigger++;
            CheckStartCondition();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInTrigger = Mathf.Max(0, playersInTrigger - 1);

            if (startRoundActive && playersInTrigger < totalPlayers)
            {
                CancelCountdown();
            }
        }
    }

    private void CheckStartCondition()
    {
        if (playersInTrigger == totalPlayers && !startRoundActive)
        {
            countdownCoroutine = StartCoroutine(StartRound());
        }
    }

    private void CancelCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        startRoundActive = false;

        if (countdownText != null)
        {
            countdownText.text = "";
            countdownText.gameObject.SetActive(false);
        }
    }

    private IEnumerator StartRound()
    {
        startRoundActive = true;

        float countdown = countdownDuration;
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        while (countdown > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.Ceil(countdown).ToString();
            }

            yield return new WaitForSeconds(1f);
            countdown -= 1f;

            if (playersInTrigger < totalPlayers)
            {
                CancelCountdown();
                yield break;
            }
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
            yield return new WaitForSeconds(1f);
            countdownText.gameObject.SetActive(false);
        }

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (teleportedPlayers.Contains(player)) continue;

            PlayerInput input = player.GetComponent<PlayerInput>();
            if (input == null) continue;

            int index = input.playerIndex;
            if (index >= teleportDestinations.Length) continue;

            Transform destination = teleportDestinations[index].transform;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.MovePosition(destination.position);
                rb.MoveRotation(destination.rotation);
            }
            else
            {
                player.transform.SetPositionAndRotation(destination.position, destination.rotation);
            }

            teleportedPlayers.Add(player);

            PlayerControls pc = player.GetComponent<PlayerControls>();
            if (pc != null)
            {
                pc.enabled = false;
                StartCoroutine(ReenableControls(pc, 0.1f));
            }
        }

        playersInTrigger = 0;

        if (scoreTextObject != null) scoreTextObject.SetActive(true);
        if (timerTextObject != null) timerTextObject.SetActive(true);

        ScoreSystem.CurrentScore = 0;
        ScoreSystem scoreSystem = FindObjectOfType<ScoreSystem>();
        if (scoreSystem != null)
        {
            scoreSystem.AddScore(0);
        }

        if (timer != null)
        {
            timer.ResetTimer(300f);
        }

        teleportedPlayers.Clear();
        startRoundActive = false;
        countdownCoroutine = null;
    }

    private IEnumerator ReenableControls(PlayerControls pc, float delay)
    {
        yield return new WaitForSeconds(delay);
        pc.enabled = true;
    }

    public void EndRound()
    {
        if (scoreTextObject != null) scoreTextObject.SetActive(false);
        if (timerTextObject != null) timerTextObject.SetActive(false);

        ScoreSystem.CurrentScore = 0;
        ScoreSystem scoreSystem = FindObjectOfType<ScoreSystem>();
        if (scoreSystem != null)
        {
            scoreSystem.AddScore(0);
        }
    }
}