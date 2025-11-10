using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Lobby zero gravity area that affects rigidbodies
/// </summary>
[RequireComponent(typeof(Collider))]
public class ZeroGravityZone : MonoBehaviour
{
    [Header("Gravity Zone Settings")]
    [SerializeField] private bool disableGravity = true;
    [SerializeField] private float zeroGDrag = 0.1f;
    [SerializeField] private float zeroGAngularDrag = 0.1f;

    private HashSet<Rigidbody> bodiesInside = new HashSet<Rigidbody>();

    private Dictionary<Rigidbody, float> originalDrag = new Dictionary<Rigidbody, float>();
    private Dictionary<Rigidbody, float> originalAngularDrag = new Dictionary<Rigidbody, float>();

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning("[ZeroGravityZone] Collider should be set as trigger", this);
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || bodiesInside.Contains(rb)) return;

        bodiesInside.Add(rb);

        if (!originalDrag.ContainsKey(rb))
        {
            originalDrag[rb] = rb.drag;
            originalAngularDrag[rb] = rb.angularDrag;
        }

        rb.useGravity = !disableGravity;
        if (disableGravity)
        {
            rb.drag = zeroGDrag;
            rb.angularDrag = zeroGAngularDrag;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb == null || !bodiesInside.Contains(rb)) return;

        bodiesInside.Remove(rb);

        rb.useGravity = true;

        if (originalDrag.TryGetValue(rb, out float drag))
        {
            rb.drag = drag;
            originalDrag.Remove(rb);
        }

        if (originalAngularDrag.TryGetValue(rb, out float angularDrag))
        {
            rb.angularDrag = angularDrag;
            originalAngularDrag.Remove(rb);
        }
    }

    private void FixedUpdate()
    {
        bodiesInside.RemoveWhere(rb => rb == null);

        if (disableGravity)
        {
            foreach (Rigidbody rb in bodiesInside)
            {
                if (rb != null)
                {
                    rb.useGravity = false;
                    rb.drag = zeroGDrag;
                    rb.angularDrag = zeroGAngularDrag;
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach (Rigidbody rb in bodiesInside)
        {
            if (rb != null)
            {
                rb.useGravity = true;

                if (originalDrag.TryGetValue(rb, out float drag))
                {
                    rb.drag = drag;
                }

                if (originalAngularDrag.TryGetValue(rb, out float angularDrag))
                {
                    rb.angularDrag = angularDrag;
                }
            }
        }

        bodiesInside.Clear();
        originalDrag.Clear();
        originalAngularDrag.Clear();
    }

    // Gizmo for visualization
    private void OnDrawGizmosSelected()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = new Color(0f, 0.5f, 1f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawSphere(sphere.center, sphere.radius);
        }
        else if (col is CapsuleCollider capsule)
        {
            Gizmos.DrawWireSphere(capsule.center, capsule.radius);
        }
    }
}