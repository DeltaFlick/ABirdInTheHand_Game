using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PickupController : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] Transform holdArea;
    private GameObject heldObj;
    private Rigidbody heldObjRB;

    [Header("Physics Parameters")]
    [SerializeField] private float pickupRange = 5.0f;
    [SerializeField] private float pickupForce = 150.0f;

    private RigidbodyConstraints originalConstraints;  // To store the original constraints

    private PlayerInput playerInput;  // Reference to the Player Input component
    private Camera playerCamera;  // Reference to the Player's Camera

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();  // Get the PlayerInput component
        playerCamera = GetComponentInChildren<Camera>();  // Get the camera attached to this player (assuming the camera is a child)
    }

    private void Update()
    {
        // Get the "Grab" action directly from the PlayerInput
        if (playerInput.actions["Grab"].triggered)  // Ensure "Grab" action is mapped in your Input Asset
        {
            if (heldObj == null)
            {
                RaycastHit hit;
                // Raycast from the center of the player's camera
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, pickupRange))
                {
                    PickupObject(hit.transform.gameObject);
                }
            }
            else
            {
                DropObject();
            }
        }

        if (heldObj != null)
        {
            MoveObject();
        }
    }

    void MoveObject()
    {
        if (Vector3.Distance(heldObj.transform.position, holdArea.position) > 0.1f)
        {
            Vector3 moveDirection = (holdArea.position - heldObj.transform.position);
            heldObjRB.AddForce(moveDirection * pickupForce);
        }
    }

    void PickupObject(GameObject pickObj)
    {
        if (pickObj.GetComponent<Rigidbody>())
        {
            heldObjRB = pickObj.GetComponent<Rigidbody>();
            heldObjRB.useGravity = false;
            heldObjRB.drag = 10;
            heldObjRB.constraints = RigidbodyConstraints.FreezeRotation;

            // Store original constraints if it's a player (assuming "Player" tag)
            if (pickObj.CompareTag("Player"))
            {
                originalConstraints = heldObjRB.constraints;
                heldObjRB.constraints = RigidbodyConstraints.FreezeAll;  // Prevent movement
            }

            heldObjRB.transform.parent = holdArea;
            heldObj = pickObj;
        }
    }

    void DropObject()
    {
        if (heldObjRB != null)
        {
            heldObjRB.useGravity = true;
            heldObjRB.drag = 1;

            // If it was a player, restore original constraints
            if (heldObj.CompareTag("Player"))
            {
                heldObjRB.constraints = originalConstraints;
            }
            else
            {
                heldObjRB.constraints = RigidbodyConstraints.None;
            }

            heldObj.transform.parent = null;
            heldObj = null;
        }
    }
}





