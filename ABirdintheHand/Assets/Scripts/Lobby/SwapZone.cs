using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSwapHandler handler = other.GetComponentInParent<PlayerSwapHandler>();
            if (handler != null)
            {
                handler.EnterSwapZone();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSwapHandler handler = other.GetComponentInParent<PlayerSwapHandler>();
            if (handler != null)
            {
                handler.ExitSwapZone();
            }
        }
    }
}

