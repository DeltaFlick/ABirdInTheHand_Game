using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdCageTrigger : MonoBehaviour
{
    private HashSet<GameObject> birdsInCage = new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<BirdIdentifier>() != null)
        {
            birdsInCage.Add(other.gameObject);
            CheckWinCondition();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<BirdIdentifier>() != null)
        {
            birdsInCage.Remove(other.gameObject);
        }
    }

    private void CheckWinCondition()
    {
        int totalBirds = BirdManager.Instance?.GetBirdCount() ?? 0;
        if (totalBirds > 0 && birdsInCage.Count == totalBirds)
        {
            Debug.Log("All birds are in the cage! Human wins!");
            // Trigger win condition
        }
    }
}


