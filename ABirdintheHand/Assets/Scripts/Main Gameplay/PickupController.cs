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

    private RigidbodyConstraints originalConstraints;

    private PlayerInput playerInput; 
    private Camera playerCamera; 

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>(); 
        playerCamera = GetComponentInChildren<Camera>(); 
    }

    private void Update()
    {
        if (playerInput.actions["Grab"].triggered)
        {
            if (heldObj == null)
            {
                RaycastHit hit;
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


            if (pickObj.CompareTag("Player"))
            {
                originalConstraints = heldObjRB.constraints;
                heldObjRB.constraints = RigidbodyConstraints.FreezeAll;
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





