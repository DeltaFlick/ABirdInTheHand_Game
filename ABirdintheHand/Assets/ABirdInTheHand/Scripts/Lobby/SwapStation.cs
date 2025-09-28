using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SwapStation : MonoBehaviour, IInteractable
{
    [Header("Swap Options")]
    public bool swapToHuman;
    public bool swapToBird;

    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    public void Interact(InteractionHandler interactor)
    {
        Debug.Log("Interacted!");

        var swapHandler = interactor.GetComponentInParent<PlayerSwapHandler>();
        if (swapHandler == null) return;

        if (swapToHuman)
            swapHandler.SwapToHuman();
        else if (swapToBird)
            swapHandler.SwapToBird();
    }

    public void ShowPrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
            if (promptText != null)
                promptText.text = swapToHuman ? "Press Interact to become Human" : "Press Interact to become Bird";
        }
    }

    public void HidePrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }
}
