using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenuUIController : MonoBehaviour
{
    public PlayerSwapHandler playerSwapHandler;

    public void OnHumanButtonPressed()
    {
        playerSwapHandler?.SwapToHuman();
    }

    public void OnBirdButtonPressed()
    {
        playerSwapHandler?.SwapToBird();
    }
}
