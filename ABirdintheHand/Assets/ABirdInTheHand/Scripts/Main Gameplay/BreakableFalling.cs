using UnityEngine;

/// <summary>
/// Break objects on falling impact and award score
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class BreakableFalling : MonoBehaviour
{
    [Header("Impact Settings")]
    [SerializeField] private float impactThreshold = 5f;
    [SerializeField] private float scoreAddAmount = 10f;
    [SerializeField] private GameObject destroyedVersion;

    [Header("Ground Detection")]
    [SerializeField] private string groundTag = "whatisGround";

    private Rigidbody rb;
    private bool hasBroken = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogError("[BreakableFalling] Rigidbody component required!", this);
            enabled = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasBroken)
            return;

        if (collision.gameObject.CompareTag(groundTag))
        {
            float impactVelocity = collision.relativeVelocity.magnitude;

            if (impactVelocity > impactThreshold)
            {
                Break();
            }
        }
    }

    private void Break()
    {
        hasBroken = true;

        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.AddScore(scoreAddAmount);
        }
        else
        {
            Debug.LogWarning("[BreakableFalling] ScoreSystem.Instance is null; score not added.", this);
        }

        ForceDrop.RequestDropAll();

        if (destroyedVersion != null)
        {
            Instantiate(destroyedVersion, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}