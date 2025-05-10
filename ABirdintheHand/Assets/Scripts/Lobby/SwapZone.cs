using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapZone : MonoBehaviour
{
    [Header("Teleport Target")]
    [SerializeField] private Transform teleportTarget;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerSwapHandler handler = other.GetComponentInParent<PlayerSwapHandler>();
            if (handler != null && teleportTarget != null)
            {
                handler.EnterSwapZone(teleportTarget);
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

