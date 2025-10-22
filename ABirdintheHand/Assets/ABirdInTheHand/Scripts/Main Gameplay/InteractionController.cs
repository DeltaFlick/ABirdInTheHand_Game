using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

/// <Summary>
/// Interaction system controller
/// </Summary>

public class InteractionController : MonoBehaviour
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
        if (playerInput != null && playerInput.actions != null)
            interactAction = playerInput.actions.FindAction("Interact", true);

        var swapHandler = GetComponent<OverlordSwapHandler>();
        if (swapHandler != null)
            swapHandler.OnVisualChanged += OnVisualChanged;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>(true);

        if (shaderSwapper == null)
            shaderSwapper = GetComponent<ShaderSwapper>();
    }

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.performed += TryInteract;
    }

    private void OnDisable()
    {
        if (interactAction != null)
            interactAction.performed -= TryInteract;
    }

    private void Update()
    {
        if (playerCamera == null) return;

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
                    shaderSwapper.ChangeShader(hit.collider.gameObject, playerCamera);
                }
                return;
            }
        }

        if (currentInteractable != null)
        {
            currentInteractable.HidePrompt();
            shaderSwapper.RemoveCamera(playerCamera);
            currentInteractable = null;
        }
    }

    private void TryInteract(InputAction.CallbackContext context)
    {
        if (currentInteractable != null)
            currentInteractable.Interact(this);
    }

    private void OnDestroy()
    {
        var swapHandler = GetComponent<OverlordSwapHandler>();
        if (swapHandler != null)
            swapHandler.OnVisualChanged -= OnVisualChanged;
    }

    private void OnVisualChanged(GameObject newVisual)
    {
        if (newVisual == null) return;

        CameraHolder holder = newVisual.GetComponentInChildren<CameraHolder>(true);
        if (holder != null)
            playerCamera = GetComponentInChildren<Camera>(true);

        if (shaderSwapper != null)
            shaderSwapper.ResetVisualReference(newVisual);
    }
}
