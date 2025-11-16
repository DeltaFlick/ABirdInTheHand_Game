using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Human & Bird Player Controls with movement, jumping, and ladder climbing
/// </summary>
[RequireComponent(typeof(Rigidbody), typeof(PlayerInput))]
public class PlayerControls : MonoBehaviour
{
    private InputActionAsset inputAsset;
    private InputActionMap playerMap;
    private InputAction move;
    private InputAction look;
    private InputAction jumpAction;

    public bool isWalking { get; private set; }
    public bool isJumping { get; private set; }

    public bool walkingAnim { get; private set; }
    public bool jumpingAnim { get; private set; }

    private Rigidbody rb;
    [SerializeField] private float movementForce = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;

    public PlayerInput pi { get; private set; }
    [SerializeField] private Camera playerCamera;
    private Transform currentVisual;
    private BirdIdentifier birdIdentifier;
    private OverlordSwapHandler swapHandler;

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Friction Settings")]
    [SerializeField] private float counterSlidingForce = 0.1f;

    [Header("Air Control Settings")]
    [SerializeField] private float airControlMultiplier = 0.2f;

    [Header("Caged Movement Settings")]
    [SerializeField] private float cagedSpeedMultiplier = 0.5f;
    [SerializeField] private float cagedJumpMultiplier = 0.5f;

    [Header("Ladder Settings")]
    [SerializeField] private float climbSpeed = 3f;
    private bool isOnLadder = false;

    private Vector3 lastLookDir = Vector3.forward;
    private bool isGroundedCache;
    private CameraController cameraController;
    private static readonly Vector3 gravityCompensation = Physics.gravity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pi = GetComponent<PlayerInput>();

        if (pi == null || pi.actions == null)
        {
            Debug.LogError("[PlayerControls] PlayerInput or actions missing!", this);
            enabled = false;
            return;
        }

        inputAsset = pi.actions;
        playerMap = inputAsset.FindActionMap("Player");

        if (playerMap == null)
        {
            Debug.LogError("[PlayerControls] 'Player' action map not found!", this);
            enabled = false;
            return;
        }

        move = playerMap.FindAction("Move");
        look = playerMap.FindAction("Look");
        jumpAction = playerMap.FindAction("Jump");

        birdIdentifier = GetComponent<BirdIdentifier>();

        swapHandler = GetComponent<OverlordSwapHandler>();
        if (swapHandler != null)
        {
            swapHandler.OnVisualChanged += HandleVisualChanged;
        }
    }

    private void Start()
    {
        if (playerCamera != null)
        {
            cameraController = playerCamera.GetComponent<CameraController>();
        }
    }

    private void HandleVisualChanged(GameObject newVisual)
    {
        currentVisual = newVisual != null ? newVisual.transform : null;

        if (playerCamera == null)
        {
            playerCamera = GetComponentInChildren<Camera>(true);

            if (playerCamera != null)
            {
                cameraController = playerCamera.GetComponent<CameraController>();
            }
        }
    }

    private void OnEnable()
    {
        if (jumpAction != null)
        {
            jumpAction.started += DoJump;
        }

        if (playerMap != null)
        {
            playerMap.Enable();
        }
    }

    private void OnDisable()
    {
        if (jumpAction != null)
        {
            jumpAction.started -= DoJump;
        }

        if (playerMap != null)
        {
            playerMap.Disable();
        }
    }

    public void SetControlsEnabled(bool enabled)
    {
        if (pi?.actions == null) return;

        InputActionMap map = pi.actions.FindActionMap("Player");
        if (map != null)
        {
            if (enabled)
                map.Enable();
            else
                map.Disable();
        }
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic || playerCamera == null || move == null)
            return;

        Vector2 moveInput = move.ReadValue<Vector2>();

        if (isOnLadder)
        {
            HandleLadderMovement(moveInput);
            return;
        }

        isGroundedCache = IsGrounded();

        float speedMultiplier = 1f;
        if (birdIdentifier != null && birdIdentifier.IsCaged)
        {
            speedMultiplier = cagedSpeedMultiplier;
        }

        float controlMultiplier = isGroundedCache ? speedMultiplier : (speedMultiplier * airControlMultiplier);

        isWalking = moveInput.sqrMagnitude > 0.1f && isGroundedCache;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            Vector3 rightDir = GetCameraRight(playerCamera);
            Vector3 forwardDir = GetCameraForward(playerCamera);

            forceDirection = (rightDir * moveInput.x + forwardDir * moveInput.y) * movementForce * controlMultiplier;
            rb.AddForce(forceDirection, ForceMode.Impulse);
        }

        if (rb.velocity.y < 0f)
        {
            rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;
        }

        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0;
        float horizontalSpeed = horizontalVelocity.magnitude;

        if (horizontalSpeed > maxSpeed)
        {
            rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;
        }

        if (isGroundedCache && moveInput.sqrMagnitude < 0.01f)
        {
            Vector3 counterForce = -rb.velocity;
            counterForce.y = 0f;
            rb.AddForce(counterForce * counterSlidingForce, ForceMode.Acceleration);
        }

        isJumping = !isGroundedCache && rb.velocity.y > 0.1f;

        LookAt();

        UpdateAnimationBools();
    }

    private void HandleLadderMovement(Vector2 moveInput)
    {
        Vector3 climbDirection = Vector3.up * moveInput.y * climbSpeed;

        Vector3 sideMove = GetCameraRight(playerCamera) * moveInput.x * (climbSpeed * 0.5f);

        rb.velocity = climbDirection + sideMove;

        Vector3 lookDirection = -playerCamera.transform.forward;
        lookDirection.y = 0;

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
        }
    }

    private void UpdateAnimationBools()
    {
        walkingAnim = isWalking;
        jumpingAnim = isJumping;
    }

    private void LookAt()
    {
        if (playerCamera == null || currentVisual == null)
            return;

        Vector3 camDir = playerCamera.transform.forward;
        camDir.y = 0f;

        if (camDir.sqrMagnitude > 0.01f)
        {
            lastLookDir = camDir.normalized;
        }

        Quaternion targetRotation = Quaternion.LookRotation(lastLookDir, Vector3.up);
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 10f * Time.fixedDeltaTime));
    }

    private Vector3 GetCameraForward(Camera cam)
    {
        if (cameraController != null && cameraController.orientation != null)
        {
            return cameraController.orientation.forward;
        }
        return cam.transform.forward;
    }

    private Vector3 GetCameraRight(Camera cam)
    {
        if (cameraController != null && cameraController.orientation != null)
        {
            return cameraController.orientation.right;
        }
        return cam.transform.right;
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
            {
                jumpMultiplier = cagedJumpMultiplier;
            }

            rb.AddForce(Vector3.up * jumpForce * jumpMultiplier, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        if (groundCheck == null)
            return false;

        return Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundMask);
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

    private void OnDestroy()
    {
        if (swapHandler != null)
        {
            swapHandler.OnVisualChanged -= HandleVisualChanged;
        }
    }

    private void OnDrawGizmos()
    {
        if (groundCheck != null)
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}