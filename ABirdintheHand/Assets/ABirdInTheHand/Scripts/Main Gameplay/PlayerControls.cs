using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Human & Bird Player Controls
/// </summary>

[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerControls : MonoBehaviour
{
    private InputActionAsset inputAsset;
    private InputActionMap playerMap;
    private InputAction move;
    private InputAction look;
    public bool isWalking = false;
    public bool isJumping = false; 
    //public bool isWiggling = false;

    //outgoing animation variables
    public bool walkingAnim = false;
    public bool jumpingAnim = false;
    //public bool wigglingAnim = false; 

    private Rigidbody rb;
    [SerializeField] private float movementForce = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;

    public PlayerInput pi { get; private set; }
    [SerializeField] public Camera playerCamera;

    private Transform currentVisual;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Friction Settings")]
    [SerializeField] private float counterSlidingForce = 0.1f;

    [Header("Caged Movement Settings")]
    [SerializeField] private float cagedSpeedMultiplier = 0.5f;
    [SerializeField] private float cagedJumpMultiplier = 0.5f;

    [Header("Ladder Settings")]
    [SerializeField] private float climbSpeed = 3f;
    private bool isOnLadder = false;

    private BirdIdentifier birdIdentifier;
    private Vector3 lastLookDir = Vector3.forward;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pi = GetComponent<PlayerInput>();

        inputAsset = pi.actions;
        playerMap = inputAsset.FindActionMap("Player");

        move = playerMap.FindAction("Move");
        look = playerMap.FindAction("Look");

        birdIdentifier = GetComponent<BirdIdentifier>();

        var swapHandler = GetComponent<OverlordSwapHandler>();
        if (swapHandler != null)
            swapHandler.OnVisualChanged += HandleVisualChanged;
    }

    private void HandleVisualChanged(GameObject newVisual)
    {
        currentVisual = newVisual != null ? newVisual.transform : null;

        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>(true);
    }

    private void OnEnable()
    {
        playerMap.FindAction("Jump").started += DoJump;
        playerMap.Enable();
    }

    private void OnDisable()
    {
        playerMap.FindAction("Jump").started -= DoJump;
        playerMap.Disable();
    }

    public void SetControlsEnabled(bool enabled)
    {
        if (enabled)
            pi.actions.FindActionMap("Player")?.Enable();
        else
            pi.actions.FindActionMap("Player")?.Disable();
    }

    private void UpdateAnimationBools()
    {
        //dunno if this is the best way but, it does work
        switch (isWalking)
        {
            case true:
                walkingAnim = true;
                break;
            case false:
                walkingAnim = false;
                break;
        }

        switch (isJumping)
        {
            case true:
                jumpingAnim = true;
                break;
            case false:
                jumpingAnim = false;
                break;
        }

        // Uncomment when wiggling is implemented
        /*
        switch (isWiggling)
        {
            case true:
                wigglingAnim = true;
                break;
            case false:
                wigglingAnim = false;
                break;
        }
        */
    }

    private void FixedUpdate()
    {

        if (rb.isKinematic) return;
        if (playerCamera == null || move == null) return;

        Vector2 moveInput = move.ReadValue<Vector2>();


        if (isOnLadder)
        {
            Vector3 climbDirection = Vector3.up * moveInput.y;
            rb.velocity = climbDirection * climbSpeed;

            Vector3 sideMove = GetCameraRight(playerCamera) * moveInput.x * (climbSpeed * 0.5f);
            rb.velocity += sideMove;

            Vector3 lookDirection = -playerCamera.transform.forward;
            lookDirection.y = 0;
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
            }

            return;
        }

        float speedMultiplier = 1f;
        if (birdIdentifier != null && birdIdentifier.IsCaged)
            speedMultiplier = cagedSpeedMultiplier;

        isWalking = moveInput.sqrMagnitude > 0.1f && IsGrounded();
        Vector3 horizontalVel = new Vector3(rb.velocity.x, 0, rb.velocity.z);

        if (!IsGrounded() && horizontalVel.magnitude < 0.1f)
        {           
             float airControlMultiplier = 0.2f;

            if (isMovingIntoWall(moveInput))
            {
                forceDirection += moveInput.x * GetCameraRight(playerCamera) * movementForce * speedMultiplier * airControlMultiplier;
            }
            else
            {
                forceDirection += moveInput.y * GetCameraForward(playerCamera) * movementForce * speedMultiplier * airControlMultiplier;
            }
        }
        else
        {
            forceDirection += moveInput.x * GetCameraRight(playerCamera) * movementForce * speedMultiplier;
            forceDirection += moveInput.y * GetCameraForward(playerCamera) * movementForce * speedMultiplier;
        }
        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        if (rb.velocity.y < 0f)
            rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;

        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;

        if (IsGrounded())
        {
            Vector3 counterForce = -rb.velocity;
            counterForce.y = 0f;
            rb.AddForce(counterForce * counterSlidingForce, ForceMode.Acceleration);
        }
        isJumping = !IsGrounded() && rb.velocity.y > 0.1f;
        LookAt();
        UpdateAnimationBools();

    }

    private bool isMovingIntoWall(Vector2 moveInput)
    {
       
        Vector3 horizontalVelocity = rb.velocity.normalized;
        horizontalVelocity.y = 0;
        if (moveInput.x > horizontalVelocity.x + 0.1f)
            return true;

        if (moveInput.y > horizontalVelocity.y + 0.1f)
            return true;

        return false;
    }
    
    private void LookAt()
    {
        if (playerCamera == null || currentVisual == null) return;

        Vector3 camDir = playerCamera.transform.forward;
        camDir.y = 0f;

        if (camDir.sqrMagnitude > 0.01f)
            lastLookDir = camDir.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(lastLookDir, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
    }


    private Vector3 GetCameraForward(Camera playerCamera)
    {
        var controller = playerCamera.GetComponent<CameraController>();
        return controller != null ? controller.orientation.forward : playerCamera.transform.forward;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        var controller = playerCamera.GetComponent<CameraController>();
        return controller != null ? controller.orientation.right : playerCamera.transform.right;
    }

    private void DoJump(InputAction.CallbackContext context)
    {
        if (isOnLadder)
        {
            isOnLadder = false;
            rb.useGravity = true;
            rb.velocity = Vector3.up * jumpForce;
            return;
        }

        if (IsGrounded() && !rb.isKinematic)
        {
            float jumpMultiplier = 1f;
            if (birdIdentifier != null && birdIdentifier.IsCaged)
                jumpMultiplier = cagedJumpMultiplier;

            forceDirection += Vector3.up * jumpForce * jumpMultiplier;
        }
    }

    private bool IsGrounded()
    {
        return groundCheck != null && Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }

    private void OnDrawGizmos()
{
    if (groundCheck != null)
    {
        Gizmos.color = IsGrounded() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}

    #region Ladder Climbing

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isOnLadder = false;
            rb.useGravity = true;
        }
    }

    #endregion
}
