using UnityEngine;

/// <summary>
/// Forces a child visual transform to follow a target transform with optional smoothing
/// </summary>
public class FollowParentRoot : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float positionSmooth = 0f;
    [SerializeField] private float rotationSmooth = 0f;

    private float positionSmoothFactor;
    private float rotationSmoothFactor;
    private bool hasTarget;

    public void SetTarget(Transform t)
    {
        target = t;
        hasTarget = target != null;
    }

    private void Start()
    {
        hasTarget = target != null;
    }

    private void LateUpdate()
    {
        if (!hasTarget) return;

        if (positionSmooth <= 0f)
        {
            transform.position = target.position;
        }
        else
        {
            positionSmoothFactor = 1f - Mathf.Exp(-positionSmooth * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, target.position, positionSmoothFactor);
        }

        if (rotationSmooth <= 0f)
        {
            transform.rotation = target.rotation;
        }
        else
        {
            rotationSmoothFactor = 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, rotationSmoothFactor);
        }
    }
}