using UnityEngine;

public class BirdIdentifier : MonoBehaviour
{
    public bool IsBeingHeld { get; set; }
    public bool IsCaged { get; set; }

    public PlayerMenuController MenuController { get; set; }

    void Awake()
    {
        if (MenuController == null)
        {
            MenuController = GetComponentInParent<PlayerMenuController>();
        }
    }

    void Start()
    {
        BirdManager.Instance?.RegisterBird(this.gameObject);
    }

    void OnDestroy()
    {
        BirdManager.Instance?.UnregisterBird(this.gameObject);
    }

    public static BirdIdentifier GetFromOverlord(GameObject root)
    {
        if (root == null) return null;

        OverlordSwapHandler swapHandler = root.GetComponent<OverlordSwapHandler>();
        if (swapHandler != null && swapHandler.CurrentVisual != null)
            return swapHandler.CurrentVisual.GetComponentInChildren<BirdIdentifier>();

        return root.GetComponentInChildren<BirdIdentifier>();
    }
}
