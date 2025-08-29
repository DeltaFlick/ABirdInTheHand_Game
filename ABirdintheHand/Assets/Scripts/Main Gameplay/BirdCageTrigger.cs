using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BirdCageTrigger : MonoBehaviour
{
    private HashSet<GameObject> birdsInCage = new HashSet<GameObject>();
    private Coroutine rescueRoutine;

    [Header("Rescue Settings")]
    [SerializeField] private float rescueTime = 3f;
    [SerializeField] private Transform releasePoint;

    private void OnTriggerEnter(Collider other)
    {
        BirdIdentifier bird = other.GetComponent<BirdIdentifier>();
        if (bird != null)
        {
            if (bird.IsBeingHeld)
            {
                if (!birdsInCage.Contains(other.gameObject))
                {
                    birdsInCage.Add(other.gameObject);
                    CheckWinCondition();
                }
            }
            else
            {
                if (rescueRoutine == null)
                    rescueRoutine = StartCoroutine(RescueCountdown());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BirdIdentifier bird = other.GetComponent<BirdIdentifier>();
        if (bird != null)
        {
            if (!bird.IsBeingHeld && rescueRoutine != null)
            {
                StopCoroutine(rescueRoutine);
                rescueRoutine = null;
            }

            if (birdsInCage.Contains(other.gameObject))
            {
                birdsInCage.Remove(other.gameObject);
            }
        }
    }

    private IEnumerator RescueCountdown()
    {
        float timer = 0f;
        while (timer < rescueTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        ReleaseAllBirds();
        rescueRoutine = null;
    }

    private void ReleaseAllBirds()
    {
        foreach (GameObject bird in birdsInCage)
        {
            if (bird != null)
            {
                Rigidbody rb = bird.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = false;

                if (releasePoint != null)
                {
                    bird.transform.position = releasePoint.position;
                    bird.transform.rotation = releasePoint.rotation;
                }

                BirdIdentifier id = bird.GetComponent<BirdIdentifier>();
                if (id != null) id.IsBeingHeld = false;
            }
        }

        birdsInCage.Clear();
    }

    private void CheckWinCondition()
    {
        int totalBirds = BirdManager.Instance?.GetBirdCount() ?? 0;

        if (totalBirds > 0 && birdsInCage.Count == totalBirds)
        {
            SceneManager.LoadScene("Humans1Win");
        }
    }
}
