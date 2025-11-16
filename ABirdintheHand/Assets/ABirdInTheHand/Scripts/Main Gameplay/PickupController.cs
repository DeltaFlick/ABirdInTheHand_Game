using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Pickup system controller with spring joint physics
/// </summary>
public class PickupController : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private Transform holdArea;
    [SerializeField] private float pickupRange = 5f;
    [SerializeField] private float spring = 500f;
    [SerializeField] private float damper = 30f;

    [Header("Crosshair Feedback")]
    [SerializeField] private Image crosshair;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite highlightSprite;
    [SerializeField] private Sprite heldSprite;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private Color heldColor = Color.green;

    private GameObject heldObj;
    private Rigidbody heldObjRB;
    private SpringJoint joint;
    private RigidbodyConstraints originalConstraints;
    private bool canPickup = false;

    private PlayerInput playerInput;
    private Camera playerCamera;
    private InputAction grabAction;
    private GameObject currentVisual;
    private OverlordSwapHandler swapHandler;

    private int pickupLayerMask;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        swapHandler = GetComponent<OverlordSwapHandler>();
        if (swapHandler != null)
        {
            swapHandler.OnVisualChanged += OnVisualChanged;

            if (swapHandler.CurrentVisual != null)
            {
                OnVisualChanged(swapHandler.CurrentVisual);
            }
        }

        pickupLayerMask = ~LayerMask.GetMask("Ignore Raycast");
    }

    private void Start()
    {
        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);
        }

        if (holdArea == null && playerCamera != null)
        {
            Transform found = playerCamera.transform.Find("HoldArea");
            if (found != null)
            {
                holdArea = found;
            }
            else
            {
                Debug.LogWarning("[PickupController] No HoldArea found. Creating default.", this);
                GameObject holdAreaObj = new GameObject("HoldArea");
                holdAreaObj.transform.SetParent(playerCamera.transform);
                holdAreaObj.transform.localPosition = new Vector3(0, 0, 2f);
                holdArea = holdAreaObj.transform;
            }
        }

        if (playerInput?.actions != null)
        {
            grabAction = playerInput.actions.FindAction("Grab", false);
        }
    }

    private void OnEnable()
    {
        ForceDrop.ForceDropAll += DropObject;
    }

    private void OnDisable()
    {
        ForceDrop.ForceDropAll -= DropObject;
    }

    private void Update()
    {
        if (!canPickup || grabAction == null)
            return;

        if (grabAction.triggered)
        {
            if (heldObj == null)
                TryPickup();
            else
                DropObject();
        }

        HandleCrosshairFeedback();
    }

    private void FixedUpdate()
    {
        if (joint != null && holdArea != null)
        {
            joint.connectedAnchor = holdArea.position;
        }
    }

    private void TryPickup()
    {
        if (playerCamera == null || holdArea == null)
            return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayerMask))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb == null)
                return;

            bool isPlayer = rb.transform.root.CompareTag("Player") ||
                           rb.transform.root.GetComponent<OverlordSwapHandler>() != null;

            if (isPlayer)
            {
                heldObj = rb.transform.root.gameObject;
                heldObjRB = rb.transform.root.GetComponent<Rigidbody>();
            }
            else
            {
                heldObj = rb.gameObject;
                heldObjRB = rb;
            }

            if (heldObjRB == null)
                return;

            BirdIdentifier bird = heldObj.GetComponentInChildren<BirdIdentifier>();
            if (bird != null)
            {
                bird.IsBeingHeld = true;
            }

            heldObjRB.useGravity = true;
            heldObjRB.drag = 4f;
            heldObjRB.interpolation = RigidbodyInterpolation.Interpolate;
            heldObjRB.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            joint = heldObj.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = holdArea.position;
            joint.spring = spring;
            joint.damper = damper;
            joint.maxDistance = 0.1f;
            joint.minDistance = 0f;

            Vector3 localHitPoint = heldObj.transform.InverseTransformPoint(hit.point);
            joint.anchor = localHitPoint;

            if (isPlayer)
            {
                originalConstraints = heldObjRB.constraints;
                heldObjRB.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }
    }

    public void DropObject()
    {
        if (joint != null)
        {
            Destroy(joint);
            joint = null;
        }

        if (heldObjRB != null)
        {
            BirdIdentifier bird = heldObj.GetComponentInChildren<BirdIdentifier>();
            if (bird != null)
            {
                bird.IsBeingHeld = false;
            }

            heldObjRB.drag = 1f;

            if (heldObj.CompareTag("Player"))
            {
                heldObjRB.constraints = originalConstraints;
            }
            else
            {
                heldObjRB.constraints = RigidbodyConstraints.None;
            }
        }

        heldObj = null;
        heldObjRB = null;
    }

    private void OnVisualChanged(GameObject newVisual)
    {
        currentVisual = newVisual;
        playerCamera = GetComponentInChildren<Camera>(true);

        if (playerCamera != null)
        {
            Transform foundHoldArea = playerCamera.transform.Find("HoldArea");
            if (foundHoldArea != null)
            {
                holdArea = foundHoldArea;
            }
            else
            {
                Debug.LogWarning($"[PickupController] No HoldArea found under {playerCamera.name}", this);
            }
        }

        bool isHuman = newVisual != null && newVisual.GetComponent<HumanIdentifier>() != null;
        canPickup = isHuman;

        if (!isHuman && heldObj != null)
        {
            DropObject();
        }

        Debug.Log($"[PickupController] Pickup {(isHuman ? "enabled" : "disabled")} for {newVisual?.name}");
    }

    private void HandleCrosshairFeedback()
    {
        if (crosshair == null || playerCamera == null)
            return;

        if (!canPickup)
        {
            SetCrosshair(defaultSprite, defaultColor);
            return;
        }

        if (heldObj != null)
        {
            SetCrosshair(heldSprite, heldColor);
            return;
        }

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, pickupLayerMask))
        {
            if (hit.rigidbody != null)
            {
                SetCrosshair(highlightSprite, highlightColor);
                return;
            }
        }

        SetCrosshair(defaultSprite, defaultColor);
    }

    private void SetCrosshair(Sprite sprite, Color color)
    {
        if (crosshair.sprite != sprite)
            crosshair.sprite = sprite;

        if (crosshair.color != color)
            crosshair.color = color;
    }

    private void OnDestroy()
    {
        DropObject();

        if (swapHandler != null)
        {
            swapHandler.OnVisualChanged -= OnVisualChanged;
        }
    }
}