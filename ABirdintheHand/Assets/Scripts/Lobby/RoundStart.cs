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
    
    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playersInTrigger++;
            CheckStartCondition();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playersInTrigger--;
            if (playersInTrigger < 0)
            {
                playersInTrigger = 0;
            }
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

                player.transform.position = teleportDestinations[randomIndex].transform.position;
            }
        }
        playersInTrigger = 0;
        startRoundActive = true;

        if (timer != null)
        {
            timer.ResetTimer(10f);
        }
    }
}