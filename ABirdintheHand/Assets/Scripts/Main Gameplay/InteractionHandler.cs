using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class InteractionHandler : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private Camera playerCamera;

    [SerializeField] private ShaderSwapper shaderSwapper;

    private PlayerInput playerInput;
    private InputAction interactAction;
    private IInteractable currentInteractable;



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
        if (shaderSwapper == null)
        {
            shaderSwapper = GetComponent<ShaderSwapper>();
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

    private void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactableMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    if (currentInteractable != null)
                    {
                        currentInteractable.HidePrompt();

                        shaderSwapper.RevertShader();
                    }
                    currentInteractable = interactable;
                    currentInteractable.ShowPrompt();

                    shaderSwapper.ChangeShader(hit.collider.gameObject);
                    
                }
                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable.HidePrompt();
            shaderSwapper.RevertShader();

            currentInteractable = null;
        }
    }

    private void TryInteract(InputAction.CallbackContext context)
    {
        if (currentInteractable != null)
            currentInteractable.Interact(this);
    }
}
