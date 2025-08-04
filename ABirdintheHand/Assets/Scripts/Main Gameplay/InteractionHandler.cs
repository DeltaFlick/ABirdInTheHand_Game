using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionHandler : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private Camera playerCamera;

    private PlayerInput playerInput;
    private InputAction interactAction;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        interactAction = playerInput.actions["Interact"];

        if (playerCamera == null)
        {
            PlayerControls pc = GetComponent<PlayerControls>();
            if (pc != null)
                playerCamera = pc.playerCamera;
        }
    }

    private void OnEnable()
    {
        interactAction.performed += TryInteract;
    }

    private void OnDisable()
    {
        interactAction.performed -= TryInteract;
    }

    private void TryInteract(InputAction.CallbackContext context)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactable.Interact();
            }
        }
    }
}
