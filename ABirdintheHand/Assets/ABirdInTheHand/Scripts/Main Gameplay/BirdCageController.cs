using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <Summary>
/// Trapping & Releasing Birds
/// </Summary>

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
    private GameObject birdToTeleport;

    private void OnTriggerEnter(Collider other)
    {
        BirdIdentifier bird = other.GetComponent<BirdIdentifier>();
        if (bird == null) return;

        if (bird.IsBeingHeld)
        {
            TeleportToCage(bird.gameObject);
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
        BirdIdentifier bird = other.GetComponent<BirdIdentifier>();
        if (bird == null) return;

        if (bird.IsCaged) return;

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

    private void TeleportToCage(GameObject bird)
    {
        if (cageSpawnPoint == null)
        {
            Debug.LogWarning("[BirdCageController] Cage spawn point not assigned!");
            return;
        }

        birdToTeleport = bird;
        Rigidbody rb = bird.GetComponent<Rigidbody>();
        BirdIdentifier birdId = bird.GetComponent<BirdIdentifier>();

        if (rb != null) rb.isKinematic = true;

        bird.transform.position = cageSpawnPoint.position;
        bird.transform.rotation = cageSpawnPoint.rotation;
        Physics.SyncTransforms();

        if (birdId != null)
        {
            birdId.IsCaged = true;
            birdId.IsBeingHeld = false;
        }

        Invoke(nameof(EnableBirdMovement), teleportDelay);
        RegisterCagedBird(birdId);
    }

    private void EnableBirdMovement()
    {
        if (birdToTeleport != null)
        {
            Rigidbody rb = birdToTeleport.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = false;
        }
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

    #endregion

    private void CheckWinCondition()
    {
        // Placeholder for total bird count
        int totalBirds = 5; // TODO: replace with BirdManager.Instance.GetBirdCount()

        if (birdsInCage.Count == totalBirds)
        {
            Debug.Log("[BirdCageController] All birds caged, humans win!");
            if (!string.IsNullOrEmpty(humansWinSceneName))
                SceneManager.LoadScene(humansWinSceneName);
        }
    }
}
