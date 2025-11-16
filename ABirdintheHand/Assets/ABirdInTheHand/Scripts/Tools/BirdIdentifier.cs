using UnityEngine;

/// <summary>
/// Identifies a bird character and tracks its state (held, caged)
/// </summary>
public class BirdIdentifier : MonoBehaviour
{
    public bool IsBeingHeld { get; set; }
    public bool IsCaged { get; set; }
    public PlayerMenuController MenuController { get; set; }

    private bool isRegistered = false;

    private void Awake()
    {
        if (MenuController == null)
        {
            MenuController = GetComponentInParent<PlayerMenuController>();
        }
    }

    private void Start()
    {
        RegisterWithManager();
    }

    private void RegisterWithManager()
    {
        if (isRegistered) return;

        if (BirdManager.Instance != null)
        {
            BirdManager.Instance.RegisterBird(gameObject);
            isRegistered = true;
        }
        else
        {
            Debug.LogWarning("[BirdIdentifier] BirdManager instance not found! Bird not registered.", this);
        }
    }

    private void OnDestroy()
    {
        if (isRegistered && BirdManager.Instance != null)
        {
            BirdManager.Instance.UnregisterBird(gameObject);
            isRegistered = false;
        }
    }


    public static BirdIdentifier GetFromOverlord(GameObject root)
    {
        if (root == null) return null;

        OverlordSwapHandler swapHandler = root.GetComponent<OverlordSwapHandler>();

        if (swapHandler != null && swapHandler.CurrentVisual != null)
        {
            return swapHandler.CurrentVisual.GetComponentInChildren<BirdIdentifier>();
        }

        return root.GetComponentInChildren<BirdIdentifier>();
    }
}