using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdLose : MonoBehaviour
{
    public int totalPlayers;
    public int playersInTrigger = 0;
    
    void OnTriggerEnter (Collider other)
    {
        if (other.gameObject.CompareTag("BirdPlayer"))
        {
            playersInTrigger++;
            CheckLossCondition();
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
            CheckLossCondition();
        }
    }

    void CheckLossCondition()
    {
        if (playersInTrigger == totalPlayers)
        {
            EndGame();
        }
    }

    void EndGame()
    {
        Debug.Log("You Lose!");
    }
}
