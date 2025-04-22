using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
    public float scoreAddAmount = 10;
    
    ScoreSystem scoreSystem;


    public GameObject destroyedVersion;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        scoreSystem = GameObject.FindGameObjectWithTag("GameController").GetComponent<ScoreSystem>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "whatisGround") 
        {
            float impactVelocity = collision.relativeVelocity.magnitude;
            if (impactVelocity > 1) 
            {
                scoreSystem.AddScore(scoreAddAmount);

                Instantiate(destroyedVersion, transform.position, transform.rotation);               
                Destroy(gameObject);
            }
        }
    }
}