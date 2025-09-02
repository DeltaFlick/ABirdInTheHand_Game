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

    [Header("Gizmo Settings")]
    [SerializeField] private Color gizmoColor = Color.green;

    private List<BirdIdentifier> birdsInCage = new List<BirdIdentifier>();
    private HashSet<BirdIdentifier> freeBirdsInTrigger = new HashSet<BirdIdentifier>();
    private Coroutine rescueRoutine;
    private BirdIdentifier currentRescuer;

    private void OnTriggerEnter(Collider other)
    {
        BirdIdentifier bird = other.GetComponent<BirdIdentifier>();
        if (bird == null) return;

        Debug.Log($"[BirdCageTrigger] Bird entered trigger: {bird.name}");

        if (!bird.IsCaged)
        {
            freeBirdsInTrigger.Add(bird);
            Debug.Log($"[BirdCageTrigger] Free bird in trigger: {bird.name} | Total free: {freeBirdsInTrigger.Count}");

            if (birdsInCage.Count > 0 && rescueRoutine == null)
            {
                Debug.Log("[BirdCageTrigger] Starting rescue countdown (free bird entered while cage occupied)");
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

        Debug.Log($"[BirdCageTrigger] Bird exited trigger: {bird.name}");

        if (freeBirdsInTrigger.Contains(bird))
        {
            freeBirdsInTrigger.Remove(bird);
            Debug.Log($"[BirdCageTrigger] Free bird removed from trigger: {bird.name} | Remaining free: {freeBirdsInTrigger.Count}");

            if (bird == currentRescuer && rescueRoutine != null)
            {
                Debug.Log("[BirdCageTrigger] Rescuer left, stopping rescue");
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

        Debug.Log($"[BirdCageTrigger] Caging bird: {bird.name}");
        PickupManager.RequestDropAll();

        bird.IsBeingHeld = false;
        bird.IsCaged = true;

        if (!birdsInCage.Contains(bird))
        {
            birdsInCage.Add(bird);
            Debug.Log($"[BirdCageTrigger] Bird added to cage list: {bird.name}");
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
            Debug.Log($"[BirdCageTrigger] Bird registered in cage: {bird.name}");
            CheckWinCondition();
        }
    }

    private IEnumerator RescueCountdown(BirdIdentifier rescuer)
    {
        float timer = 0f;
        Debug.Log("[BirdCageTrigger] Rescue timer started");

        rescuer.MenuController?.ShowRescueTimer(rescueTime);

        while (timer < rescueTime)
        {
            if (freeBirdsInTrigger.Count == 0 || birdsInCage.Count == 0)
            {
                Debug.Log("[BirdCageTrigger] Rescue timer stopped (conditions not met)");
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

        Debug.Log("[BirdCageTrigger] Rescue timer completed, releasing all birds");
        ReleaseAllBirds();

        rescuer.MenuController?.HideRescueTimer();
        rescueRoutine = null;
        currentRescuer = null;
    }

    private void ReleaseAllBirds()
    {
        Debug.Log("[BirdCageTrigger] Releasing all birds from cage");

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

            Debug.Log($"[BirdCageTrigger] Bird released: {bird.name}");
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

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = gizmoColor;

            if (col is BoxCollider box)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
            else if (col is SphereCollider sphere)
            {
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }

        if (releasePoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(releasePoint.position, 0.5f);
        }
    }
}
