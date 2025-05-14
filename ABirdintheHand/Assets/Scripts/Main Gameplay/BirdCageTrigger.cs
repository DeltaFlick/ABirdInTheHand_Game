using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BirdCageTrigger : MonoBehaviour
{
    private HashSet<GameObject> birdsInCage = new HashSet<GameObject>();

    private void OnTriggerEnter(Collider other)
    {

        BirdIdentifier bird = other.GetComponent<BirdIdentifier>();
        if (bird != null)
        {
            if (!birdsInCage.Contains(other.gameObject))
            {
                birdsInCage.Add(other.gameObject);
                CheckWinCondition();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BirdIdentifier bird = other.GetComponent<BirdIdentifier>();
        if (bird != null)
        {
            if (birdsInCage.Contains(other.gameObject))
            {
                birdsInCage.Remove(other.gameObject);
            }
        }
    }

    private void CheckWinCondition()
    {
        int totalBirds = BirdManager.Instance?.GetBirdCount() ?? 0;

        if (totalBirds > 0 && birdsInCage.Count == totalBirds)
        {
            SceneManager.LoadScene("HumansWin");
        }
    }
}
