using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenuUIController : MonoBehaviour
{
    public PlayerSwapHandler playerSwapHandler;

    public void OnHumanButtonPressed()
    {
        if (playerSwapHandler != null)
        {
            playerSwapHandler.SwapToHuman();
        }
    }

    public void OnBirdButtonPressed()
    {
        if (playerSwapHandler != null)
        {
            playerSwapHandler.SwapToBird();
        }
    }
}
