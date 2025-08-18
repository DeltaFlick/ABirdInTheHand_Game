using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void Interact(InteractionHandler interactor);
    void ShowPrompt();
    void HidePrompt();
}
