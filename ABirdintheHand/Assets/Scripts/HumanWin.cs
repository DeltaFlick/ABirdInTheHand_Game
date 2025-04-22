using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanWin : MonoBehaviour
{
    public int totalPlayers;
    public int playersInTrigger = 0;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BirdPlayer"))
        {
            playersInTrigger++;
            CheckWinCondition();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("BirdPlayer"))
        {
            playersInTrigger--;
            if (playersInTrigger < 0)
            {
                playersInTrigger = 0;
            }
            CheckWinCondition();
        }
    }

    void CheckWinCondition()
    {
        if (playersInTrigger == totalPlayers)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        Debug.Log("You Win!");
    }
}