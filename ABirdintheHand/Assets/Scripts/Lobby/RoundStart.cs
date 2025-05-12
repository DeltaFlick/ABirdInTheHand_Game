using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private HashSet<GameObject> teleportedPlayers = new HashSet<GameObject>();

    [Header("UI Elements")]
    public GameObject scoreTextObject;
    public GameObject timerTextObject;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInTrigger++;
            Debug.Log($"Player entered trigger. Players in trigger: {playersInTrigger}");
            CheckStartCondition();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersInTrigger = Mathf.Max(0, playersInTrigger - 1);
            Debug.Log($"Player exited trigger. Players in trigger: {playersInTrigger}");
        }
    }

    private void CheckStartCondition()
    {
        if (playersInTrigger == totalPlayers && !startRoundActive)
        {
            StartCoroutine(StartRound());
        }
    }

    private IEnumerator StartRound()
    {
        startRoundActive = true;

        yield return null;

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

        // Activate UI
        if (scoreTextObject != null) scoreTextObject.SetActive(true);
        if (timerTextObject != null) timerTextObject.SetActive(true);

        // Reset score
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

        yield return new WaitForSeconds(1f);
        teleportedPlayers.Clear();
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
            scoreSystem.AddScore(0); // Trigger UI update
        }
    }
}
