using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class GravityZone : MonoBehaviour
{
    [Header("Gravity Zone Settings")]
    [SerializeField] private bool disableGravity = true;
    [SerializeField] private float zeroGDrag = 0.1f;
    [SerializeField] private float zeroGAngularDrag = 0.1f;

    private HashSet<Rigidbody> bodiesInside = new HashSet<Rigidbody>();

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            bodiesInside.Add(rb);
            rb.useGravity = !disableGravity;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && bodiesInside.Contains(rb))
        {
            bodiesInside.Remove(rb);
            rb.useGravity = true;
        }
    }

    private void FixedUpdate()
    {
        foreach (Rigidbody rb in bodiesInside)
        {
            if (rb != null)
            {
                rb.useGravity = !disableGravity;
                if (disableGravity)
                {
                    rb.drag = zeroGDrag;
                    rb.angularDrag = zeroGAngularDrag;
                }
            }
        }
    }


    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(0f, 0.5f, 1f, 0.25f);
            Gizmos.matrix = transform.localToWorldMatrix;
            if (col is BoxCollider box)
                Gizmos.DrawCube(box.center, box.size);
            else if (col is SphereCollider sphere)
                Gizmos.DrawSphere(sphere.center, sphere.radius);
            else if (col is CapsuleCollider capsule)
                Gizmos.DrawWireSphere(capsule.center, capsule.radius);
        }
    }
}
