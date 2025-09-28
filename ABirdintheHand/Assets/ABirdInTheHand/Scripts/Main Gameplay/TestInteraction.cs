using System;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TestInteraction : MonoBehaviour, IInteractable
{
    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    public void Interact(InteractionHandler interactor)
    {
        Debug.Log("Interacted!");
    }

    public void ShowPrompt()
    {
        if (promptUI != null) promptUI.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }
}