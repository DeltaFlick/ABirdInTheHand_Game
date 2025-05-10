using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundStart : MonoBehaviour
{
    public GameObject[] teleportDestinations;
    public Timer timer;
    public int totalPlayers;
    public int playersInTrigger = 0;

    public bool startRoundActive = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playersInTrigger++;
            Debug.Log($"Player entered trigger. Players in trigger: {playersInTrigger}");
            CheckStartCondition();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playersInTrigger--;
            if (playersInTrigger < 0)
                playersInTrigger = 0;

            Debug.Log($"Player exited trigger. Players in trigger: {playersInTrigger}");
            CheckStartCondition();
        }
    }

    void CheckStartCondition()
    {
        if (playersInTrigger == totalPlayers)
        {
            StartRound();
        }
    }

    public void StartRound()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (teleportDestinations.Length > 0)
            {
                int randomIndex = Random.Range(0, teleportDestinations.Length);
                Vector3 newPos = teleportDestinations[randomIndex].transform.position;

                Debug.Log($"Teleporting {player.name} to index {randomIndex}: {newPos}");

                Rigidbody rb = player.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                    rb.transform.position = newPos;
                    rb.MovePosition(newPos);
                }
                else
                {
                    player.transform.position = newPos;
                }

                // Optional: temporarily disable input to prevent immediate movement
                PlayerControls pc = player.GetComponent<PlayerControls>();
                if (pc != null)
                {
                    pc.enabled = false;
                    StartCoroutine(ReenableControls(pc, 0.1f));
                }
            }
        }

        playersInTrigger = 0;
        startRoundActive = true;

        if (timer != null)
        {
            timer.ResetTimer(10f);
        }
    }

    private IEnumerator ReenableControls(PlayerControls pc, float delay)
    {
        yield return new WaitForSeconds(delay);
        pc.enabled = true;
    }
}
