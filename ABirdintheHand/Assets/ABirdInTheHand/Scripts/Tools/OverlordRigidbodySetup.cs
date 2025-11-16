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

        if (rb == null)
        {
            Debug.LogError("[OverlordRigidbodySetup] Rigidbody component required!", this);
            enabled = false;
        }
    }

    private void Start()
    {
        StartCoroutine(SetupRigidbodyAfterFrame());
    }

    private IEnumerator SetupRigidbodyAfterFrame()
    {
        yield return new WaitForEndOfFrame();

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotationX |
                           RigidbodyConstraints.FreezeRotationY |
                           RigidbodyConstraints.FreezeRotationZ;

            Debug.Log($"[OverlordRigidbodySetup] Configured rigidbody for {gameObject.name}", this);
        }
    }
}