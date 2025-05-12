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

        // Wait a frame to allow character swaps to complete
        yield return null;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (teleportedPlayers.Contains(player)) continue;

            PlayerInput input = player.GetComponent<PlayerInput>();
            if (input == null)
            {
                Debug.LogWarning($"Player {player.name} is missing PlayerInput component.");
                continue;
            }

            int index = input.playerIndex;

            if (index >= teleportDestinations.Length)
            {
                Debug.LogWarning($"No teleport destination for player index {index}");
                continue;
            }

            Transform destination = teleportDestinations[index].transform;
            Vector3 newPos = destination.position;
            Quaternion newRot = destination.rotation;

            Debug.Log($"Teleporting {player.name} (Player {index}) to: {newPos}");

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.MovePosition(newPos);
                rb.MoveRotation(newRot);
            }
            else
            {
                player.transform.SetPositionAndRotation(newPos, newRot);
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

        if (timer != null)
        {
            timer.ResetTimer(300f);
        }

        yield return new WaitForSeconds(1f); // Optional delay before resetting tracking
        teleportedPlayers.Clear();
        startRoundActive = false;
    }

    private IEnumerator ReenableControls(PlayerControls pc, float delay)
    {
        yield return new WaitForSeconds(delay);
        pc.enabled = true;
    }
}
