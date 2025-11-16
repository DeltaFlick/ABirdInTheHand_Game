using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Interaction system controller with raycast-based object detection
/// </summary>
public class InteractionController : MonoBehaviour
{
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableMask;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ShaderSwapper shaderSwapper;

    [Header("Performance Settings")]
    [SerializeField] private bool enableRaycastThrottling = false;
    [SerializeField] private float raycastInterval = 0.05f;

    private PlayerInput playerInput;
    private InputAction interactAction;
    private IInteractable currentInteractable;
    private GameObject currentHighlightedObject;
    private float lastRaycastTime;
    private OverlordSwapHandler swapHandler;

    private Ray interactionRay;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput?.actions != null)
        {
            interactAction = playerInput.actions.FindAction("Interact", true);
        }

        swapHandler = GetComponent<OverlordSwapHandler>();
        if (swapHandler != null)
        {
            swapHandler.OnVisualChanged += OnVisualChanged;
        }

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }

        if (shaderSwapper == null)
        {
            shaderSwapper = GetComponent<ShaderSwapper>();
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.performed += TryInteract;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.performed -= TryInteract;
        }

        ClearCurrentInteractable();
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        if (enableRaycastThrottling)
        {
            if (Time.time - lastRaycastTime < raycastInterval)
                return;

            lastRaycastTime = Time.time;
        }

        interactionRay.origin = playerCamera.transform.position;
        interactionRay.direction = playerCamera.transform.forward;

        if (Physics.Raycast(interactionRay, out RaycastHit hit, interactionDistance, interactableMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                if (currentInteractable != interactable)
                {
                    ClearCurrentInteractable();

                    currentInteractable = interactable;
                    currentHighlightedObject = hit.collider.gameObject;

                    currentInteractable.ShowPrompt();

                    if (shaderSwapper != null)
                    {
                        shaderSwapper.ChangeShader(currentHighlightedObject, playerCamera);
                    }
                }
                return;
            }
        }

        if (currentInteractable != null)
        {
            ClearCurrentInteractable();
        }
    }

    private void TryInteract(InputAction.CallbackContext context)
    {
        if (currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }
    }

    private void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            currentInteractable.HidePrompt();
            currentInteractable = null;
        }

        if (shaderSwapper != null)
        {
            if (currentHighlightedObject != null)
            {
                shaderSwapper.RevertShader();
            }

            if (playerCamera != null)
            {
                shaderSwapper.RemoveCamera(playerCamera);
            }
        }

        currentHighlightedObject = null;
    }

    private void OnVisualChanged(GameObject newVisual)
    {
        if (newVisual == null)
            return;

        CameraHolder holder = newVisual.GetComponentInChildren<CameraHolder>(true);
        if (holder != null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }

        if (shaderSwapper != null)
        {
            shaderSwapper.ResetVisualReference(newVisual);
        }

        ClearCurrentInteractable();
    }

    private void OnDestroy()
    {
        ClearCurrentInteractable();

        if (swapHandler != null)
        {
            swapHandler.OnVisualChanged -= OnVisualChanged;
        }
    }
}