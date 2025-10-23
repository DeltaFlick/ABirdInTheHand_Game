using UnityEngine;
using TMPro;

/// <summary>
/// Interact with a station to swap to a character prefab
/// </summary>

public class SwapStation : MonoBehaviour, IInteractable
{
    [Header("Swap Options")]
    [SerializeField] private GameObject swapPrefab;

    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    private string GetPromptText()
    {
        if (swapPrefab == null) return "Swap Unavailable";

        string type = swapPrefab.GetComponent<HumanIdentifier>() != null ? "Human" :
                      swapPrefab.GetComponent<BirdIdentifier>() != null ? "Bird" : "Character";

        return $"Press Interact to become {type}";
    }

    public void Interact(InteractionController interactor)
    {
        if (swapPrefab == null || interactor == null) return;

        var swapHandler = interactor.GetComponentInParent<OverlordSwapHandler>();
        if (swapHandler == null) return;

        GameObject targetPrefab = swapHandler.GetPrefabByReference(swapPrefab);
        if (targetPrefab != null)
        {
            swapHandler.SwapToCharacter(targetPrefab.name);
            Debug.Log($"[SwapStation] {interactor.name} swapped to {swapPrefab.name}");
        }
        else
        {
            Debug.LogWarning($"[SwapStation] SwapPrefab {swapPrefab.name} not found in OverlordSwapHandler's characterPrefabs list.");
        }
    }

    public void ShowPrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
            if (promptText != null)
                promptText.text = GetPromptText();
        }
    }

    public void HidePrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }
}
