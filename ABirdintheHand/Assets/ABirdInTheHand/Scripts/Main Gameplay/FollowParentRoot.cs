using UnityEngine;

/// <summary>
/// Forces a child visual transform to follow the provided target transform every LateUpdate.
/// </summary>

public class FollowParentRoot : MonoBehaviour
{
    [SerializeField] private Transform target;

    [SerializeField] private float positionSmooth = 0f;
    [SerializeField] private float rotationSmooth = 0f;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        if (positionSmooth <= 0f)
            transform.position = target.position;
        else
            transform.position = Vector3.Lerp(transform.position, target.position, 1f - Mathf.Exp(-positionSmooth * Time.deltaTime));

        if (rotationSmooth <= 0f)
            transform.rotation = target.rotation;
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime));
    }
}
