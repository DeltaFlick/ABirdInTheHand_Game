using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundStart : MonoBehaviour
{
    public Vector3 teleportDestination;
    public int totalPlayers;
    public int playersInTrigger = 0;
    
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

    void StartRound()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            player.transform.position = teleportDestination;
        }
        playersInTrigger = 0; // Reset player count
    }
}