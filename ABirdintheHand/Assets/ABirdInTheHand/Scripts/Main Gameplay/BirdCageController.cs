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
    [SerializeField] private float rescueTime = 10f;

    [Header("Win Condition")]
    [SerializeField] private string humansWinSceneName = "HumansWin";

    [Header("Debug")]
    [SerializeField] private bool verboseLogging = false;

    private List<BirdIdentifier> birdsInCage = new List<BirdIdentifier>();
    private Dictionary<GameObject, BirdIdentifier> freeBirdsInTrigger = new Dictionary<GameObject, BirdIdentifier>();
    private Coroutine rescueRoutine;
    private BirdIdentifier currentRescuer;
    private bool isRescuing = false;
    private float rescueStartTime;

    private WaitForFixedUpdate waitForFixedUpdate;
    private int cachedTotalBirds = -1;

    private void Awake()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();
    }

    private void OnTriggerEnter(Collider other)
    {
        BirdIdentifier bird = BirdIdentifier.GetFromOverlord(other.gameObject);
        if (bird == null) return;

        GameObject birdRoot = bird.transform.root.gameObject;

        if (verboseLogging)
            Debug.Log($"[BirdCage] OnTriggerEnter: {birdRoot.name}, IsBeingHeld: {bird.IsBeingHeld}, IsCaged: {bird.IsCaged}", this);

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
                Debug.Log($"[BirdCage] Added free bird. Total free: {freeBirdsInTrigger.Count}, Caged: {birdsInCage.Count}", this);
        }

        TryStartRescue();
    }

    private void OnTriggerExit(Collider other)
    {
        BirdIdentifier bird = BirdIdentifier.GetFromOverlord(other.gameObject);
        if (bird == null) return;

        GameObject birdRoot = bird.transform.root.gameObject;

        if (verboseLogging)
            Debug.Log($"[BirdCage] OnTriggerExit: {birdRoot.name}, IsCaged: {bird.IsCaged}", this);

        if (bird.IsCaged)
            return;

        if (freeBirdsInTrigger.ContainsKey(birdRoot))
        {
            freeBirdsInTrigger.Remove(birdRoot);

            if (verboseLogging)
                Debug.Log($"[BirdCage] Removed free bird. Total free: {freeBirdsInTrigger.Count}", this);

            if (bird == currentRescuer && isRescuing)
            {
                if (verboseLogging)
                    Debug.Log("[BirdCage] Current rescuer left trigger, canceling rescue", this);
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
                    Debug.Log($"[BirdCage] Removed {birdRoot.name} from free birds (Stay check). Total free: {freeBirdsInTrigger.Count}", this);

                if (bird == currentRescuer && isRescuing)
                {
                    if (verboseLogging)
                        Debug.Log("[BirdCage] Current rescuer became invalid (Stay), canceling rescue", this);
                    CancelRescue();
                }
            }
        }
        else if (!freeBirdsInTrigger.ContainsKey(birdRoot))
        {
            freeBirdsInTrigger.Add(birdRoot, bird);

            if (verboseLogging)
                Debug.Log($"[BirdCage] Re-added {birdRoot.name} to free birds (Stay check). Total free: {freeBirdsInTrigger.Count}", this);

            TryStartRescue();
        }
    }

    private void TryStartRescue()
    {
        if (isRescuing || freeBirdsInTrigger.Count == 0 || birdsInCage.Count == 0)
            return;

        currentRescuer = freeBirdsInTrigger.Values.FirstOrDefault();

        if (currentRescuer != null)
        {
            isRescuing = true;
            rescueStartTime = Time.time;
            rescueRoutine = StartCoroutine(RescueCountdown(currentRescuer));

            if (verboseLogging)
                Debug.Log($"[BirdCage] Started rescue with {currentRescuer.transform.root.name}", this);
        }
    }

    private void TeleportToCage(GameObject birdRoot, BirdIdentifier bird)
    {
        if (cageSpawnPoint == null)
        {
            Debug.LogWarning("[BirdCageController] Cage spawn point not assigned!", this);
            return;
        }

        if (verboseLogging)
            Debug.Log($"[BirdCage] Teleporting {birdRoot.name} to cage", this);

        StartCoroutine(TeleportSequence(birdRoot, bird));
    }

    private IEnumerator TeleportSequence(GameObject birdRoot, BirdIdentifier bird)
    {
        Rigidbody rootRb = birdRoot.GetComponent<Rigidbody>();
        RigidbodyInterpolation originalInterpolation = RigidbodyInterpolation.None;

        if (rootRb != null)
        {
            originalInterpolation = rootRb.interpolation;
            rootRb.interpolation = RigidbodyInterpolation.None;
            rootRb.velocity = Vector3.zero;
            rootRb.angularVelocity = Vector3.zero;
        }

        Rigidbody[] allRigidbodies = birdRoot.GetComponentsInChildren<Rigidbody>();
        foreach (var rb in allRigidbodies)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        birdRoot.transform.position = cageSpawnPoint.position;
        birdRoot.transform.rotation = cageSpawnPoint.rotation;
        Physics.SyncTransforms();

        yield return waitForFixedUpdate;
        yield return waitForFixedUpdate;

        if (rootRb != null)
        {
            rootRb.velocity = Vector3.zero;
            rootRb.angularVelocity = Vector3.zero;
        }

        foreach (var rb in allRigidbodies)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        yield return waitForFixedUpdate;

        if (rootRb != null)
        {
            rootRb.interpolation = originalInterpolation;
        }

        bird.IsCaged = true;
        bird.IsBeingHeld = false;

        // Remove from free birds list
        if (freeBirdsInTrigger.ContainsKey(birdRoot))
        {
            freeBirdsInTrigger.Remove(birdRoot);

            if (verboseLogging)
                Debug.Log("[BirdCage] Removed caged bird from free list", this);
        }

        if (bird == currentRescuer && isRescuing)
        {
            if (verboseLogging)
                Debug.Log("[BirdCage] Caged bird was rescuer, canceling rescue", this);
            CancelRescue();
        }

        RegisterCagedBird(bird);
    }

    #region Rescue Mechanic

    private void RegisterCagedBird(BirdIdentifier bird)
    {
        if (bird == null || birdsInCage.Contains(bird))
            return;

        ForceDrop.RequestDropAll();

        birdsInCage.Add(bird);

        if (verboseLogging)
            Debug.Log($"[BirdCage] Registered caged bird. Total caged: {birdsInCage.Count}", this);

        CheckWinCondition();
    }

    private IEnumerator RescueCountdown(BirdIdentifier rescuer)
    {
        if (rescuer == null)
        {
            CancelRescue();
            yield break;
        }

        float timer = 0f;
        GameObject rescuerRoot = rescuer.transform.root.gameObject;

        RescueEvents.OnRescueStarted?.Invoke(rescuer.MenuController, rescueTime);

        while (timer < rescueTime)
        {
            if (rescuer == null || !freeBirdsInTrigger.ContainsKey(rescuerRoot))
            {
                if (verboseLogging)
                    Debug.Log($"[BirdCage] Rescue failed: rescuer no longer in trigger (elapsed: {timer:F1}s)", this);
                CancelRescue();
                yield break;
            }

            if (rescuer.IsCaged || rescuer.IsBeingHeld)
            {
                if (verboseLogging)
                    Debug.Log($"[BirdCage] Rescue failed: rescuer state changed (elapsed: {timer:F1}s)", this);
                CancelRescue();
                yield break;
            }

            if (birdsInCage.Count == 0)
            {
                if (verboseLogging)
                    Debug.Log($"[BirdCage] Rescue failed: no birds left to rescue (elapsed: {timer:F1}s)", this);
                CancelRescue();
                yield break;
            }

            timer += Time.deltaTime;
            float timeLeft = rescueTime - timer;

            RescueEvents.OnRescueUpdated?.Invoke(rescuer.MenuController, timeLeft);

            yield return null;
        }

        if (verboseLogging)
            Debug.Log($"[BirdCage] *** RESCUE COMPLETED *** by {rescuerRoot.name}. Releasing {birdsInCage.Count} birds.", this);

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
                Debug.Log($"[BirdCage] Rescue canceled after {elapsed:F1}s", this);

            RescueEvents.OnRescueEnded?.Invoke(currentRescuer.MenuController);
        }

        isRescuing = false;
        currentRescuer = null;

        if (freeBirdsInTrigger.Count > 0 && birdsInCage.Count > 0)
        {
            if (verboseLogging)
                Debug.Log($"[BirdCage] Attempting to restart rescue with {freeBirdsInTrigger.Count} free bird(s)", this);

            TryStartRescue();
        }
    }

    private void ReleaseAllBirds()
    {
        if (releasePoint == null)
        {
            Debug.LogWarning("[BirdCageController] Release point not assigned!", this);
            return;
        }

        int releaseCount = birdsInCage.Count;
        List<BirdIdentifier> birdsToRelease = new List<BirdIdentifier>(birdsInCage);

        foreach (var bird in birdsToRelease)
        {
            if (bird == null)
            {
                Debug.LogWarning("[BirdCage] Null bird in cage list during release!", this);
                continue;
            }

            GameObject birdRoot = bird.transform.root.gameObject;

            Rigidbody rootRb = birdRoot.GetComponent<Rigidbody>();
            if (rootRb != null)
            {
                rootRb.velocity = Vector3.zero;
                rootRb.angularVelocity = Vector3.zero;
            }

            foreach (var rb in birdRoot.GetComponentsInChildren<Rigidbody>())
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            birdRoot.transform.position = releasePoint.position;
            birdRoot.transform.rotation = releasePoint.rotation;
            Physics.SyncTransforms();

            bird.IsCaged = false;
            bird.IsBeingHeld = false;

            if (verboseLogging)
                Debug.Log($"[BirdCage] Released {birdRoot.name}", this);
        }

        birdsInCage.Clear();

        Debug.Log($"[BirdCage] *** RELEASED {releaseCount} BIRDS ***", this);
    }

    #endregion

    private void CheckWinCondition()
    {
        if (cachedTotalBirds < 0 && BirdManager.Instance != null)
        {
            cachedTotalBirds = BirdManager.Instance.GetBirdCount();
        }

        if (cachedTotalBirds <= 0)
        {
            Debug.LogWarning("[BirdCageController] No birds registered in BirdManager!", this);
            return;
        }

        if (birdsInCage.Count >= cachedTotalBirds)
        {
            Debug.Log("[BirdCageController] All birds caged, humans win!", this);

            if (!string.IsNullOrEmpty(humansWinSceneName))
            {
                SceneManager.LoadScene(humansWinSceneName);
            }
            else
            {
                Debug.LogWarning("[BirdCageController] Humans win scene name not set!", this);
            }
        }
    }

    private void OnDestroy()
    {
        if (rescueRoutine != null)
        {
            StopCoroutine(rescueRoutine);
            rescueRoutine = null;
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
        Debug.Log("=== CAGE STATUS ===");
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