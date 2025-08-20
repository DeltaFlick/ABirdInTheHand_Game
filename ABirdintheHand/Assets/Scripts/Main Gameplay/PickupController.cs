using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {
        if (playerInput.actions["Grab"].triggered)
        {
            if (heldObj == null)
            {
                TryPickup();
            }
            else
            {
                DropObject();
            }
        }

        HandleCrosshairFeedback();
    }

    void FixedUpdate()
    {
        if (joint != null)
        {
            joint.connectedAnchor = holdArea.position;
        }
    }

    void TryPickup()
    {
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, pickupRange))
        {
            Rigidbody rb = hit.rigidbody;
            if (rb != null)
            {
                heldObj = rb.gameObject;
                heldObjRB = rb;

                BirdIdentifier bird = heldObj.GetComponent<BirdIdentifier>();
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

                if (heldObj.CompareTag("Player"))
                {
                    originalConstraints = heldObjRB.constraints;
                    heldObjRB.constraints = RigidbodyConstraints.FreezeRotation;
                }
            }
        }
    }

    void DropObject()
    {
        if (joint != null)
        {
            Destroy(joint);
        }

        if (heldObjRB != null)
        {

            BirdIdentifier bird = heldObj.GetComponent<BirdIdentifier>();
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
        joint = null;
    }

    void HandleCrosshairFeedback()
    {
        if (crosshair == null)
            return;

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
