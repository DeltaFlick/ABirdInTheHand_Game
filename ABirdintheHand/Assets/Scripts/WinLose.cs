using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinLose : MonoBehaviour
{
    private bool gameEnded;

    public void WinLevel()
    {
        if (!gameEnded)
        {

            Debug.Log("You Win!");
                gameEnded = true;
        }
    }

    public void LoseLevel()
    {
        if (!gameEnded)
        {

            Debug.Log("You Lose!");
                gameEnded = true;
        }
    }
}
