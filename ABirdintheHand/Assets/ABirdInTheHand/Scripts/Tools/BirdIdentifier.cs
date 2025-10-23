using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Identify Birds through script
/// </summary>
public class BirdIdentifier : MonoBehaviour
{
    public bool IsBeingHeld { get; set; }
    public bool IsCaged { get; set; }
    public PlayerMenuController MenuController { get; private set; }

    void Awake()
    {
        MenuController = GetComponent<PlayerMenuController>();
        if (MenuController == null)
            MenuController = GetComponentInChildren<PlayerMenuController>();
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
            return swapHandler.CurrentVisual.GetComponent<BirdIdentifier>();
        return root.GetComponent<BirdIdentifier>();
    }
}
