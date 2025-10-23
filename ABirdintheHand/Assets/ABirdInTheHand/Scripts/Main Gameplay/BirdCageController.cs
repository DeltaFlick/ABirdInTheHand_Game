using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Trapping & Releasing Birds
/// </summary>

public class BirdCageController : MonoBehaviour
{
    [Header("Cage Settings")]
    [SerializeField] private Transform cageSpawnPoint;
    [SerializeField] private Transform releasePoint;
    [SerializeField] private float teleportDelay = 0.1f;
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
        BirdIdentifier bird = BirdIdentifier.GetFromOverlord(other.gameObject);
        if (bird == null) return;

        GameObject birdRoot = bird.transform.root.gameObject;

        if (bird.IsBeingHeld)
        {
            TeleportToCage(birdRoot, bird);
            return;
        }

        if (!bird.IsCaged)
        {
            freeBirdsInTrigger.Add(bird);

            if (birdsInCage.Count > 0 && rescueRoutine == null)
            {
                currentRescuer = bird;
                rescueRoutine = StartCoroutine(RescueCountdown(bird));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BirdIdentifier bird = BirdIdentifier.GetFromOverlord(other.gameObject);
        if (bird == null || bird.IsCaged) return;

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

    private void TeleportToCage(GameObject birdRoot, BirdIdentifier bird)
    {
        if (cageSpawnPoint == null)
        {
            Debug.LogWarning("[BirdCageController] Cage spawn point not assigned!");
            return;
        }

        foreach (var rb in birdRoot.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;

        birdRoot.transform.position = cageSpawnPoint.position;
        birdRoot.transform.rotation = cageSpawnPoint.rotation;
        Physics.SyncTransforms();

        bird.IsCaged = true;
        bird.IsBeingHeld = false;

        StartCoroutine(ReenablePhysics(birdRoot));

        RegisterCagedBird(bird);
    }

    private IEnumerator ReenablePhysics(GameObject birdRoot)
    {
        yield return new WaitForSeconds(teleportDelay);
        foreach (var rb in birdRoot.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = false;
    }

    #region Rescue Mechanic

    private void RegisterCagedBird(BirdIdentifier bird)
    {
        if (bird == null || birdsInCage.Contains(bird)) return;

        ForceDrop.RequestDropAll();

        birdsInCage.Add(bird);
        CheckWinCondition();
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

            GameObject birdRoot = bird.transform.root.gameObject;

            foreach (var rb in birdRoot.GetComponentsInChildren<Rigidbody>())
                rb.isKinematic = false;

            if (releasePoint != null)
            {
                birdRoot.transform.position = releasePoint.position;
                birdRoot.transform.rotation = releasePoint.rotation;
            }

            bird.IsCaged = false;
            bird.IsBeingHeld = false;
        }

        birdsInCage.Clear();
    }

    #endregion

    private void CheckWinCondition()
    {
        // Placeholder for total bird count
        int totalBirds = 5; // TODO: replace with BirdManager.Instance.GetBirdCount()

        if (birdsInCage.Count == totalBirds)

        //int totalBirds = BirdManager.Instance?.GetBirdCount() ?? 0;

        //if (birdsInCage.Count == totalBirds && totalBirds > 0)
        {
            Debug.Log("[BirdCageController] All birds caged, humans win!");
            if (!string.IsNullOrEmpty(humansWinSceneName))
                SceneManager.LoadScene(humansWinSceneName);
        }
    }
}