using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{

    public GameObject destroyedVersion;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "whatisGround") 
        {
            float impactVelocity = collision.relativeVelocity.magnitude;
            if (impactVelocity > 1) 
            {
                Instantiate(destroyedVersion, transform.position, transform.rotation);               
                Destroy(gameObject);
            }
        }
    }
}