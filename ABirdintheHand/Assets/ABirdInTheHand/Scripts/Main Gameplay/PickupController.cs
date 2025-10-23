using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Pickup system controller
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
    private PlayerInput playerInput;
    private Camera playerCamera;
    private RigidbodyConstraints originalConstraints;
    private bool canPickup = false;
    private GameObject currentVisual;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        var swapHandler = GetComponent<OverlordSwapHandler>();
        if (swapHandler != null)
        {
            swapHandler.OnVisualChanged += OnVisualChanged;

            if (swapHandler.CurrentVisual != null)
                OnVisualChanged(swapHandler.CurrentVisual);
        }
    }

    private void Start()
    {
        if (holdArea == null && playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>(true);

        if (holdArea == null && playerCamera != null)
        {
            Transform found = playerCamera.transform.Find("HoldArea");
            if (found != null)
                holdArea = found;
        }
    }

    private void OnEnable() => ForceDrop.ForceDropAll += DropObject;
    private void OnDisable() => ForceDrop.ForceDropAll -= DropObject;

    private void Update()
    {
        if (!canPickup || playerInput == null || playerInput.actions == null) return;

        var grabAction = playerInput.actions.FindAction("Grab", false);
        if (grabAction == null) return;

        if (grabAction.triggered)
        {
            if (heldObj == null) TryPickup();
            else DropObject();
        }

        HandleCrosshairFeedback();
    }

    private void FixedUpdate()
    {
        if (joint != null && holdArea != null)
            joint.connectedAnchor = holdArea.position;
    }

    private void TryPickup()
    {
        if (playerCamera == null) return;

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, pickupRange))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb != null)
            {
                heldObj = rb.transform.root.gameObject;
                heldObjRB = rb.transform.root.GetComponent<Rigidbody>();
                if (heldObjRB == null) heldObjRB = rb;

                BirdIdentifier bird = heldObj.GetComponentInChildren<BirdIdentifier>();
                if (bird != null) bird.IsBeingHeld = true;

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

                if (heldObj.CompareTag("Player"))
                {
                    originalConstraints = heldObjRB.constraints;
                    heldObjRB.constraints = RigidbodyConstraints.FreezeRotation;
                }
            }
        }
    }

    public void DropObject()
    {
        if (joint != null) Destroy(joint);

        if (heldObjRB != null)
        {
            BirdIdentifier bird = heldObj.GetComponentInChildren<BirdIdentifier>();
            if (bird != null) bird.IsBeingHeld = false;

            heldObjRB.drag = 1f;

            if (heldObj.CompareTag("Player"))
                heldObjRB.constraints = originalConstraints;
            else
                heldObjRB.constraints = RigidbodyConstraints.None;
        }

        heldObj = null;
        heldObjRB = null;
        joint = null;
    }

    private void OnDestroy()
    {
        var swapHandler = GetComponent<OverlordSwapHandler>();
        if (swapHandler != null)
            swapHandler.OnVisualChanged -= OnVisualChanged;
    }

    private void OnVisualChanged(GameObject newVisual)
    {
        currentVisual = newVisual;
        playerCamera = GetComponentInChildren<Camera>(true);

        if (playerCamera != null)
        {
            Transform foundHoldArea = playerCamera.transform.Find("HoldArea");
            if (foundHoldArea != null)
                holdArea = foundHoldArea;
            else
                Debug.LogWarning($"[PickupController] No HoldArea found under {playerCamera.name}! Please create one.");
        }

        bool isHuman = newVisual != null && newVisual.GetComponent<HumanIdentifier>() != null;
        canPickup = isHuman;

        if (!isHuman) DropObject();

        Debug.Log($"[PickupController] {(isHuman ? "Enabled" : "Disabled")} for {newVisual?.name}");
    }

    private void HandleCrosshairFeedback()
    {
        if (crosshair == null || playerCamera == null) return;

        if (!canPickup)
        {
            crosshair.sprite = defaultSprite;
            crosshair.color = defaultColor;
            return;
        }

        if (heldObj != null)
        {
            crosshair.sprite = heldSprite;
            crosshair.color = heldColor;
            return;
        }

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, pickupRange))
        {
            if (hit.rigidbody != null)
            {
                crosshair.sprite = highlightSprite;
                crosshair.color = highlightColor;
                return;
            }
        }

        crosshair.sprite = defaultSprite;
        crosshair.color = defaultColor;
    }
}
