using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CageTeleportTrigger : MonoBehaviour
{
    [SerializeField] private Transform cageSpawnPoint;
    public float teleportDelay = 0.1f;

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
        Collider birdCollider = bird.GetComponent<Collider>();

        if (birdRigidbody != null && birdCollider != null)
        {
            birdRigidbody.isKinematic = true;

            bird.transform.position = cageSpawnPoint.position;
            bird.transform.rotation = cageSpawnPoint.rotation;
            Physics.SyncTransforms();

            Invoke("EnableBirdMovement", teleportDelay);
        }
    }


    private void EnableBirdMovement()
    {
        if (birdToTeleport != null)
        {
            Rigidbody birdRigidbody = birdToTeleport.GetComponent<Rigidbody>();
            Collider birdCollider = birdToTeleport.GetComponent<Collider>();

            if (birdRigidbody != null && birdCollider != null)
            {
                birdRigidbody.isKinematic = false;
            }
        }
    }
}
