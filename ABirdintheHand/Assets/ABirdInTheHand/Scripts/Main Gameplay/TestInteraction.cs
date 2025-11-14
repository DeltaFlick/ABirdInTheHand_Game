using UnityEngine;
using TMPro;
/// <summary>
/// Test interaction for future scripts
/// </summary>
public class TestInteraction : MonoBehaviour, IInteractable
{
    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;
    public void Interact(InteractionController interactor)
    {
        Debug.Log($"[TestInteraction] Interacted by {(interactor != null ? interactor.name : "Unknown")}", this);
    }
    public void ShowPrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
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