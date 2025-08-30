using System.Collections;
using UnityEngine;

public class CageTeleportTrigger : MonoBehaviour
{
    [SerializeField] private Transform cageSpawnPoint;
    [SerializeField] private float teleportDelay = 0.1f;

    private GameObject birdToTeleport;

    private void OnTriggerEnter(Collider other)
    {
        BirdIdentifier bird = other.GetComponent<BirdIdentifier>();
        if (bird != null && bird.IsBeingHeld)
        {
            TeleportToCage(other.gameObject);
        }
    }

    private void TeleportToCage(GameObject bird)
    {
        birdToTeleport = bird;
        Rigidbody birdRigidbody = bird.GetComponent<Rigidbody>();

        if (birdRigidbody != null)
        {
            birdRigidbody.isKinematic = true;
        }

        bird.transform.position = cageSpawnPoint.position;
        bird.transform.rotation = cageSpawnPoint.rotation;
        Physics.SyncTransforms();

        BirdIdentifier birdId = bird.GetComponent<BirdIdentifier>();
        if (birdId != null)
        {
            birdId.IsCaged = true;
            birdId.IsBeingHeld = false;
            Debug.Log($"[CageTeleportTrigger] Bird caged via teleport: {bird.name}");
        }

        Invoke(nameof(EnableBirdMovement), teleportDelay);

        BirdCageTrigger cageTrigger = GetComponent<BirdCageTrigger>();
        if (cageTrigger != null)
        {
            cageTrigger.RegisterCagedBird(birdId);
        }
    }

    private void EnableBirdMovement()
    {
        if (birdToTeleport != null)
        {
            Rigidbody rb = birdToTeleport.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }
        }
    }
}
