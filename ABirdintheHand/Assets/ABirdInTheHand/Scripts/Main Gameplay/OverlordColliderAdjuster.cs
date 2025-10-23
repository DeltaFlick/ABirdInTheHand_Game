using UnityEngine;

/// <summary>
/// Automatically adjusts the Overlord's collider to match the current visual's collider
/// </summary>

[RequireComponent(typeof(OverlordSwapHandler))]
public class OverlordColliderAdjuster : MonoBehaviour
{
    private OverlordSwapHandler swapHandler;
    private Collider overlordCollider;

    private void Awake()
    {
        swapHandler = GetComponent<OverlordSwapHandler>();
        overlordCollider = GetComponent<Collider>();

        if (overlordCollider == null)
        {
            overlordCollider = gameObject.AddComponent<CapsuleCollider>();
            Debug.LogWarning("[OverlordColliderAutoAdjuster] No collider found; created default CapsuleCollider.");
        }
    }

    private void OnEnable()
    {
        if (swapHandler != null)
            swapHandler.OnVisualChanged += HandleVisualChanged;
    }

    private void OnDisable()
    {
        if (swapHandler != null)
            swapHandler.OnVisualChanged -= HandleVisualChanged;
    }

    private void HandleVisualChanged(GameObject newVisual)
    {
        if (newVisual == null) return;

        Collider src = newVisual.GetComponentInChildren<Collider>();
        if (src != null)
        {
            ReplaceWithMatchingCollider(src);
            Debug.Log($"[OverlordColliderAutoAdjuster] Applied {src.GetType().Name} shape from {newVisual.name}");
        }
        else
        {
            Debug.LogWarning($"[OverlordColliderAutoAdjuster] No collider found on {newVisual.name}. Keeping existing collider.");
        }
    }

    private void ReplaceWithMatchingCollider(Collider source)
    {
        if (overlordCollider != null)
            Destroy(overlordCollider);

        overlordCollider = gameObject.AddComponent(source.GetType()) as Collider;

        overlordCollider.sharedMaterial = source.sharedMaterial;

        switch (source)
        {
            case CapsuleCollider srcCapsule:
                CapsuleCollider dstCapsule = (CapsuleCollider)overlordCollider;
                dstCapsule.center = srcCapsule.center;
                dstCapsule.height = srcCapsule.height;
                dstCapsule.radius = srcCapsule.radius;
                dstCapsule.direction = srcCapsule.direction;
                break;

            case SphereCollider srcSphere:
                SphereCollider dstSphere = (SphereCollider)overlordCollider;
                dstSphere.center = srcSphere.center;
                dstSphere.radius = srcSphere.radius;
                break;

            case BoxCollider srcBox:
                BoxCollider dstBox = (BoxCollider)overlordCollider;
                dstBox.center = srcBox.center;
                dstBox.size = srcBox.size;
                break;

            default:
                Debug.LogWarning($"[OverlordColliderAutoAdjuster] Collider type {source.GetType().Name} not specifically handled.");
                break;
        }
    }

    private void ReplaceWithCapsule(Vector3 center, float height, float radius)
    {
        if (overlordCollider != null)
            Destroy(overlordCollider);

        CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
        capsule.center = center;
        capsule.height = height;
        capsule.radius = radius;
        overlordCollider = capsule;
    }
}
