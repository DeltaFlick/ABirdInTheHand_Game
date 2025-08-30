using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    private InputActionAsset inputAsset;
    private InputActionMap playerMap;
    private InputAction move;
    private InputAction look;
    public bool isWalking = false;

    private Rigidbody rb;
    [SerializeField] private float movementForce = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;

    public PlayerInput pi { get; private set; }
    [SerializeField] public Camera playerCamera;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Friction Settings")]
    [SerializeField] private float counterSlidingForce = 0.1f;

    [Header("Caged Movement Settings")]
    [SerializeField] private float cagedSpeedMultiplier = 0.5f;
    [SerializeField] private float cagedJumpMultiplier = 0.5f;

    private BirdIdentifier birdIdentifier;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pi = GetComponent<PlayerInput>();

        inputAsset = pi.actions;
        playerMap = inputAsset.FindActionMap("Player");

        move = playerMap.FindAction("Move");
        look = playerMap.FindAction("Look");

        birdIdentifier = GetComponent<BirdIdentifier>();
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
            playerMap.Enable();
        else
            playerMap.Disable();
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic) return;

        Vector2 moveInput = move.ReadValue<Vector2>();
        float speedMultiplier = 1f;

        if (birdIdentifier != null && birdIdentifier.IsCaged)
        {
            speedMultiplier = cagedSpeedMultiplier;
        }

        isWalking = moveInput.sqrMagnitude > 0.1f && IsGrounded();

        forceDirection += moveInput.x * GetCameraRight(playerCamera) * movementForce * speedMultiplier;
        forceDirection += moveInput.y * GetCameraForward(playerCamera) * movementForce * speedMultiplier;

        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        if (rb.velocity.y < 0f)
            rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;

        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;

        if (IsGrounded() && moveInput.sqrMagnitude < 0.1f)
        {
            Vector3 counterForce = -rb.velocity;
            counterForce.y = 0f;
            rb.AddForce(counterForce * counterSlidingForce, ForceMode.Impulse);
        }

        LookAt();
    }

    private void LookAt()
    {
        Vector3 lookDirection = playerCamera.transform.forward;
        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        return playerCamera.GetComponent<HumanCamera>().orientation.forward;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        return playerCamera.GetComponent<HumanCamera>().orientation.right;
    }

    private void DoJump(InputAction.CallbackContext context)
    {
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
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }
}
