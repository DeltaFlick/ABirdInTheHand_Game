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

    private string cachedPromptText;
    private bool promptTextCached = false;

    private void Start()
    {
        CachePromptText();
    }

    private void OnValidate()
    {
        promptTextCached = false;
    }

    private void CachePromptText()
    {
        if (swapPrefab == null)
        {
            cachedPromptText = "Swap Unavailable";
        }
        else
        {
            string type = "Character";

            if (swapPrefab.GetComponent<HumanIdentifier>() != null)
            {
                type = "Human";
            }
            else if (swapPrefab.GetComponent<BirdIdentifier>() != null)
            {
                type = "Bird";
            }

            cachedPromptText = $"Press Interact to become {type}";
        }

        promptTextCached = true;
    }

    public void Interact(InteractionController interactor)
    {
        if (swapPrefab == null)
        {
            Debug.LogWarning("[SwapStation] No swap prefab assigned", this);
            return;
        }

        if (interactor == null)
        {
            Debug.LogWarning("[SwapStation] Interactor is null", this);
            return;
        }

        OverlordSwapHandler swapHandler = interactor.GetComponentInParent<OverlordSwapHandler>();
        if (swapHandler == null)
        {
            Debug.LogWarning($"[SwapStation] No OverlordSwapHandler found on {interactor.name}", this);
            return;
        }

        GameObject targetPrefab = swapHandler.GetPrefabByReference(swapPrefab);

        if (targetPrefab != null)
        {
            swapHandler.SwapToCharacter(targetPrefab.name);
            Debug.Log($"[SwapStation] {interactor.name} swapped to {swapPrefab.name}", this);
        }
        else
        {
            Debug.LogWarning($"[SwapStation] Prefab '{swapPrefab.name}' not found in OverlordSwapHandler's characterPrefabs list", this);
        }
    }

    public void ShowPrompt()
    {
        if (promptUI == null) return;

        promptUI.SetActive(true);

        if (promptText != null)
        {
            if (!promptTextCached)
            {
                CachePromptText();
            }

            promptText.text = cachedPromptText;
        }
    }

    public void HidePrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }
}