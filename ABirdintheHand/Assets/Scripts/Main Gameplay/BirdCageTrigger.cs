using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BirdCageTrigger : MonoBehaviour
{
    [Header("Cage Settings")]
    [SerializeField] private Transform cageSpawnPoint;
    [SerializeField] private Transform releasePoint;
    [SerializeField] private float rescueTime = 10f;

    [Header("Win Condition")]
    [Tooltip("Scene to load when all birds are caged")]
    [SerializeField] private string humansWinSceneName = "HumansWin";

    private List<BirdIdentifier> birdsInCage = new List<BirdIdentifier>();
    private HashSet<BirdIdentifier> freeBirdsInTrigger = new HashSet<BirdIdentifier>();
    private Coroutine rescueRoutine;
    private BirdIdentifier currentRescuer;

    private void OnTriggerEnter(Collider other)
    {
        BirdIdentifier bird = other.GetComponent<BirdIdentifier>();
        if (bird == null) return;

        if (!bird.IsCaged)
        {
            freeBirdsInTrigger.Add(bird);

            if (birdsInCage.Count > 0 && rescueRoutine == null)
            {
                currentRescuer = bird;
                rescueRoutine = StartCoroutine(RescueCountdown(bird));
            }
        }

        if (bird.IsBeingHeld)
        {
            CageBird(bird);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BirdIdentifier bird = other.GetComponent<BirdIdentifier>();
        if (bird == null) return;

        if (bird.IsCaged)
            return;

        if (freeBirdsInTrigger.Contains(bird))
        {
            freeBirdsInTrigger.Remove(bird);

            if (bird == currentRescuer && rescueRoutine != null)
            {
                StopCoroutine(rescueRoutine);
                bird.MenuController?.HideRescueTimer();
                rescueRoutine = null;
                currentRescuer = null;
            }
        }
    }

    private void CageBird(BirdIdentifier bird)
    {
        if (bird == null || bird.IsCaged) return;

        PickupManager.RequestDropAll();

        bird.IsBeingHeld = false;
        bird.IsCaged = true;

        if (!birdsInCage.Contains(bird))
        {
            birdsInCage.Add(bird);
        }

        if (cageSpawnPoint != null)
        {
            Rigidbody rb = bird.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            bird.transform.position = cageSpawnPoint.position;
            bird.transform.rotation = cageSpawnPoint.rotation;
            Physics.SyncTransforms();

            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }

        CheckWinCondition();
    }

    public void RegisterCagedBird(BirdIdentifier bird)
    {
        if (bird != null && !birdsInCage.Contains(bird))
        {
            birdsInCage.Add(bird);
            CheckWinCondition();
        }
    }

    private IEnumerator RescueCountdown(BirdIdentifier rescuer)
    {
        float timer = 0f;

        rescuer.MenuController?.ShowRescueTimer(rescueTime);

        while (timer < rescueTime)
        {
            if (freeBirdsInTrigger.Count == 0 || birdsInCage.Count == 0)
            {
                rescuer.MenuController?.HideRescueTimer();
                rescueRoutine = null;
                currentRescuer = null;
                yield break;
            }

            timer += Time.deltaTime;
            float timeLeft = rescueTime - timer;
            rescuer.MenuController?.UpdateRescueTimer(timeLeft);

            yield return null;
        }

        ReleaseAllBirds();

        rescuer.MenuController?.HideRescueTimer();
        rescueRoutine = null;
        currentRescuer = null;
    }

    private void ReleaseAllBirds()
    {
        foreach (var bird in birdsInCage)
        {
            if (bird == null) continue;

            Rigidbody rb = bird.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = false;

            if (releasePoint != null)
            {
                bird.transform.position = releasePoint.position;
                bird.transform.rotation = releasePoint.rotation;
            }

            bird.IsCaged = false;
            bird.IsBeingHeld = false;
        }

        birdsInCage.Clear();
    }

    private void CheckWinCondition()
    {
        //int totalBirds = BirdManager.Instance?.GetBirdCount() ?? 0;

        //if (totalBirds > 0 && birdsInCage.Count == totalBirds)

        int totalBirds = 5; // temporary for testing

        if (birdsInCage.Count == totalBirds)
        {
            Debug.Log("[BirdCageTrigger] All birds caged, humans win!");
            if (!string.IsNullOrEmpty(humansWinSceneName))
            {
                SceneManager.LoadScene(humansWinSceneName);
            }
        }
    }
}