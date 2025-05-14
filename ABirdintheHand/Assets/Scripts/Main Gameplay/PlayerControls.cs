using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    private InputActionAsset inputAsset;
    private InputActionMap player;
    private InputAction move;

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

    public void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SetPlayerInputActive(bool activation, PlayerInput playerInput)
    {
        if (pi == null)
            pi = playerInput;

        pi.enabled = activation;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputAsset = GetComponent<PlayerInput>().actions;
        player = inputAsset.FindActionMap("Player");
        pi = GetComponent<PlayerInput>();
    }

    private void OnEnable()
    {
        player.FindAction("Jump").started += DoJump;
        move = player.FindAction("Move");
        player.Enable();
    }

    private void OnDisable()
    {
        player.FindAction("Jump").started -= DoJump;
        player.Disable();
    }

    private void FixedUpdate()
    {
        Vector2 moveInput = move.ReadValue<Vector2>();

        forceDirection += moveInput.x * GetCameraRight(playerCamera) * movementForce;
        forceDirection += moveInput.y * GetCameraForward(playerCamera) * movementForce;

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
        Vector3 direction = rb.velocity;
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        else
            rb.angularVelocity = Vector3.zero;
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }

    private void DoJump(InputAction.CallbackContext context)
    {
        if (IsGrounded())
        {
            forceDirection += Vector3.up * jumpForce;
        }
    }

    private bool IsGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
    }
}
