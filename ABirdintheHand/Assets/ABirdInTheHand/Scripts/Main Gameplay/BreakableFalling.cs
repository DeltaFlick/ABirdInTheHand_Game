using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <Summary>
/// Break objects on falling impact
/// </Summary>

public class BreakableFalling : MonoBehaviour
{
    public float scoreAddAmount = 10;
    public float impactThreshold = 5f;

    public GameObject destroyedVersion;

    private Rigidbody rb;
    private ScoreSystem scoreSystem;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        scoreSystem = GameObject.FindGameObjectWithTag("GameController").GetComponent<ScoreSystem>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("whatisGround"))
        {
            float impactVelocity = collision.relativeVelocity.magnitude;

            if (impactVelocity > impactThreshold)
            {
                scoreSystem.AddScore(scoreAddAmount);
                ForceDrop.RequestDropAll();
                Instantiate(destroyedVersion, transform.position, transform.rotation);
                Destroy(gameObject);
            }
        }
    }
}
