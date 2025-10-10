using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <Summary>
/// Interaction helper
/// </Summary>

public interface IInteractable
{
    void Interact(InteractionController interactor);
    void ShowPrompt();
    void HidePrompt();
}
