using System.Collections;
using UnityEngine;

/// <summary>
/// Ensures the Overlord root Rigidbody has proper physics settings
/// </summary>

[RequireComponent(typeof(Rigidbody))]
public class OverlordRigidbodySetup : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        StartCoroutine(SetupRigidbodyAfterFrame());
    }

    private IEnumerator SetupRigidbodyAfterFrame()
    {
        yield return new WaitForEndOfFrame();

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY;

        Debug.Log($"[OverlordRigidbody] Set interpolation and collision detection for {gameObject.name}");
    }
}