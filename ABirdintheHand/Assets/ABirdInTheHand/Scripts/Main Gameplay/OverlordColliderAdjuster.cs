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
            Debug.LogWarning("[OverlordColliderAdjuster] No collider found; created default CapsuleCollider.", this);
        }
    }

    private void OnEnable()
    {
        if (swapHandler != null)
        {
            swapHandler.OnVisualChanged += HandleVisualChanged;
        }
    }

    private void OnDisable()
    {
        if (swapHandler != null)
        {
            swapHandler.OnVisualChanged -= HandleVisualChanged;
        }
    }

    private void HandleVisualChanged(GameObject newVisual)
    {
        if (newVisual == null)
            return;

        Collider src = newVisual.GetComponentInChildren<Collider>();

        if (src != null)
        {
            ReplaceWithMatchingCollider(src);
            Debug.Log($"[OverlordColliderAdjuster] Applied {src.GetType().Name} shape from {newVisual.name}", this);
        }
        else
        {
            Debug.LogWarning($"[OverlordColliderAdjuster] No collider found on {newVisual.name}. Keeping existing collider.", this);
        }
    }

    private void ReplaceWithMatchingCollider(Collider source)
    {
        if (overlordCollider != null)
        {
            Destroy(overlordCollider);
        }

        overlordCollider = gameObject.AddComponent(source.GetType()) as Collider;

        if (overlordCollider == null)
        {
            Debug.LogError("[OverlordColliderAdjuster] Failed to create collider!", this);
            return;
        }

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
                Debug.LogWarning($"[OverlordColliderAdjuster] Collider type {source.GetType().Name} not specifically handled.", this);
                break;
        }
    }
}