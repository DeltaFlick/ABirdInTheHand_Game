using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles trapping and releasing birds with per-player rescue timers
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

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = true;

    private List<BirdIdentifier> birdsInCage = new List<BirdIdentifier>();
    private Dictionary<GameObject, BirdIdentifier> freeBirdsInTrigger = new Dictionary<GameObject, BirdIdentifier>();
    private Coroutine rescueRoutine;
    private BirdIdentifier currentRescuer;
    private bool isRescuing = false;
    private float rescueStartTime;

    private void OnTriggerEnter(Collider other)
    {
        BirdIdentifier bird = BirdIdentifier.GetFromOverlord(other.gameObject);
        if (bird == null) return;

        // Get the root object for tracking
        GameObject birdRoot = bird.transform.root.gameObject;

        if (verboseLogging)
            Debug.Log($"[BirdCage] OnTriggerEnter: {birdRoot.name}, IsBeingHeld: {bird.IsBeingHeld}, IsCaged: {bird.IsCaged}");

        if (bird.IsBeingHeld)
        {
            TeleportToCage(birdRoot, bird);
            return;
        }

        if (bird.IsCaged)
            return;

        if (!freeBirdsInTrigger.ContainsKey(birdRoot))
        {
            freeBirdsInTrigger.Add(birdRoot, bird);

            if (verboseLogging)
                Debug.Log($"[BirdCage] Added free bird. Total free: {freeBirdsInTrigger.Count}, Caged: {birdsInCage.Count}");
        }

        TryStartRescue();
    }

    private void OnTriggerExit(Collider other)
    {
        BirdIdentifier bird = BirdIdentifier.GetFromOverlord(other.gameObject);
        if (bird == null) return;

        GameObject birdRoot = bird.transform.root.gameObject;

        if (verboseLogging)
            Debug.Log($"[BirdCage] OnTriggerExit: {birdRoot.name}, IsCaged: {bird.IsCaged}");

        if (bird.IsCaged)
            return;

        if (freeBirdsInTrigger.ContainsKey(birdRoot))
        {
            freeBirdsInTrigger.Remove(birdRoot);

            if (verboseLogging)
                Debug.Log($"[BirdCage] Removed free bird. Total free: {freeBirdsInTrigger.Count}");

            if (bird == currentRescuer && isRescuing)
            {
                if (verboseLogging)
                    Debug.Log($"[BirdCage] Current rescuer left trigger, canceling rescue");
                CancelRescue();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        BirdIdentifier bird = BirdIdentifier.GetFromOverlord(other.gameObject);
        if (bird == null) return;

        GameObject birdRoot = bird.transform.root.gameObject;

        if (bird.IsCaged || bird.IsBeingHeld)
        {
            if (freeBirdsInTrigger.ContainsKey(birdRoot))
            {
                freeBirdsInTrigger.Remove(birdRoot);

                if (verboseLogging)
                    Debug.Log($"[BirdCage] Removed {birdRoot.name} from free birds (Stay check). Total free: {freeBirdsInTrigger.Count}");

                if (bird == currentRescuer && isRescuing)
                {
                    if (verboseLogging)
                        Debug.Log($"[BirdCage] Current rescuer became invalid (Stay), canceling rescue");
                    CancelRescue();
                }
            }
        }
        else if (!bird.IsCaged && !bird.IsBeingHeld && !freeBirdsInTrigger.ContainsKey(birdRoot))
        {
            freeBirdsInTrigger.Add(birdRoot, bird);

            if (verboseLogging)
                Debug.Log($"[BirdCage] Re-added {birdRoot.name} to free birds (Stay check). Total free: {freeBirdsInTrigger.Count}");

            TryStartRescue();
        }
    }

    private void TryStartRescue()
    {
        if (!isRescuing && freeBirdsInTrigger.Count > 0 && birdsInCage.Count > 0)
        {
            currentRescuer = freeBirdsInTrigger.Values.FirstOrDefault();

            if (currentRescuer != null)
            {
                isRescuing = true;
                rescueStartTime = Time.time;
                rescueRoutine = StartCoroutine(RescueCountdown(currentRescuer));

                if (verboseLogging)
                    Debug.Log($"[BirdCage] Started rescue with {currentRescuer.transform.root.name}");
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

        if (verboseLogging)
            Debug.Log($"[BirdCage] Teleporting {birdRoot.name} to cage");

        foreach (var rb in birdRoot.GetComponentsInChildren<Rigidbody>())
            rb.isKinematic = true;

        birdRoot.transform.position = cageSpawnPoint.position;
        birdRoot.transform.rotation = cageSpawnPoint.rotation;
        Physics.SyncTransforms();

        bird.IsCaged = true;
        bird.IsBeingHeld = false;

        if (freeBirdsInTrigger.ContainsKey(birdRoot))
        {
            freeBirdsInTrigger.Remove(birdRoot);

            if (verboseLogging)
                Debug.Log($"[BirdCage] Removed caged bird from free list");
        }

        if (bird == currentRescuer && isRescuing)
        {
            if (verboseLogging)
                Debug.Log($"[BirdCage] Caged bird was rescuer, canceling rescue");
            CancelRescue();
        }

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

        if (verboseLogging)
            Debug.Log($"[BirdCage] Registered caged bird. Total caged: {birdsInCage.Count}");

        CheckWinCondition();
    }

    private IEnumerator RescueCountdown(BirdIdentifier rescuer)
    {
        float timer = 0f;
        GameObject rescuerRoot = rescuer.transform.root.gameObject;

        RescueEvents.OnRescueStarted?.Invoke(rescuer.MenuController, rescueTime);

        while (timer < rescueTime)
        {
            if (rescuer == null || !freeBirdsInTrigger.ContainsKey(rescuerRoot))
            {
                if (verboseLogging)
                    Debug.Log($"[BirdCage] Rescue failed: rescuer no longer in trigger (elapsed: {timer:F1}s)");
                CancelRescue();
                yield break;
            }

            if (rescuer.IsCaged || rescuer.IsBeingHeld)
            {
                if (verboseLogging)
                    Debug.Log($"[BirdCage] Rescue failed: rescuer state changed (elapsed: {timer:F1}s)");
                CancelRescue();
                yield break;
            }

            if (birdsInCage.Count == 0)
            {
                if (verboseLogging)
                    Debug.Log($"[BirdCage] Rescue failed: no birds left to rescue (elapsed: {timer:F1}s)");
                CancelRescue();
                yield break;
            }

            timer += Time.deltaTime;
            float timeLeft = rescueTime - timer;

            RescueEvents.OnRescueUpdated?.Invoke(rescuer.MenuController, timeLeft);

            yield return null;
        }

        if (verboseLogging)
            Debug.Log($"[BirdCage] *** RESCUE COMPLETED *** by {rescuerRoot.name}. Releasing {birdsInCage.Count} birds.");

        ReleaseAllBirds();

        RescueEvents.OnRescueEnded?.Invoke(rescuer.MenuController);

        isRescuing = false;
        rescueRoutine = null;
        currentRescuer = null;
    }

    private void CancelRescue()
    {
        if (rescueRoutine != null)
        {
            StopCoroutine(rescueRoutine);
            rescueRoutine = null;
        }

        if (currentRescuer != null)
        {
            float elapsed = Time.time - rescueStartTime;
            if (verboseLogging)
                Debug.Log($"[BirdCage] Rescue canceled after {elapsed:F1}s");

            RescueEvents.OnRescueEnded?.Invoke(currentRescuer.MenuController);
        }

        isRescuing = false;
        currentRescuer = null;

        if (freeBirdsInTrigger.Count > 0 && birdsInCage.Count > 0)
        {
            if (verboseLogging)
                Debug.Log($"[BirdCage] Attempting to restart rescue with {freeBirdsInTrigger.Count} free bird(s)");

            TryStartRescue();
        }
    }

    private void ReleaseAllBirds()
    {
        int releaseCount = birdsInCage.Count;
        List<BirdIdentifier> birdsToRelease = new List<BirdIdentifier>(birdsInCage);

        foreach (var bird in birdsToRelease)
        {
            if (bird == null)
            {
                Debug.LogWarning("[BirdCage] Null bird in cage list during release!");
                continue;
            }

            GameObject birdRoot = bird.transform.root.gameObject;

            foreach (var rb in birdRoot.GetComponentsInChildren<Rigidbody>())
                rb.isKinematic = false;

            if (releasePoint != null)
            {
                birdRoot.transform.position = releasePoint.position;
                birdRoot.transform.rotation = releasePoint.rotation;
                Physics.SyncTransforms();
            }

            bird.IsCaged = false;
            bird.IsBeingHeld = false;

            if (verboseLogging)
                Debug.Log($"[BirdCage] Released {birdRoot.name}");
        }

        birdsInCage.Clear();

        Debug.Log($"[BirdCage] *** RELEASED {releaseCount} BIRDS ***");
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
            Debug.LogWarning("[BirdCageController] No birds registered in BirdManager!");
            return;
        }

        if (birdsInCage.Count == totalBirds)
        {
            Debug.Log("[BirdCageController] All birds caged, humans win!");
            if (!string.IsNullOrEmpty(humansWinSceneName))
                SceneManager.LoadScene(humansWinSceneName);
        }
    }

    private void OnDrawGizmos()
    {
        if (cageSpawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(cageSpawnPoint.position, 0.5f);
        }

        if (releasePoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(releasePoint.position, 0.5f);
        }
    }

    [ContextMenu("Debug: Print Cage Status")]
    private void DebugPrintStatus()
    {
        Debug.Log($"=== CAGE STATUS ===");
        Debug.Log($"Free birds in trigger: {freeBirdsInTrigger.Count}");
        Debug.Log($"Caged birds: {birdsInCage.Count}");
        Debug.Log($"Is rescuing: {isRescuing}");
        Debug.Log($"Current rescuer: {(currentRescuer != null ? currentRescuer.transform.root.name : "None")}");

        if (freeBirdsInTrigger.Count > 0)
        {
            Debug.Log("Free birds:");
            foreach (var kvp in freeBirdsInTrigger)
            {
                Debug.Log($"  - {kvp.Key.name} (Caged: {kvp.Value.IsCaged}, Held: {kvp.Value.IsBeingHeld})");
            }
        }

        if (birdsInCage.Count > 0)
        {
            Debug.Log("Caged birds:");
            foreach (var bird in birdsInCage)
            {
                if (bird != null)
                    Debug.Log($"  - {bird.transform.root.name}");
            }
        }
    }
}