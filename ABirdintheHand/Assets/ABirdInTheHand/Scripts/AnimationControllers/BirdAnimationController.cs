using UnityEngine;

public class BirdAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerControls playerControls;

    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int IsFlyingHash = Animator.StringToHash("IsFlying");
    private static readonly int WalkSpeedHash = Animator.StringToHash("WalkSpeed");
    private static readonly int FlySpeedHash = Animator.StringToHash("FlySpeed");

    [SerializeField] private float walkSpeedMultiplier = 1.0f;
    [SerializeField] private float flySpeedMultiplier = 1.0f;

    private bool wasWalking;
    private bool wasFlying;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError($"[BirdAnimationController] Animator component missing on {gameObject.name}", this);
            enabled = false;
            return;
        }

        playerControls = GetComponentInParent<PlayerControls>();

        if (playerControls == null)
        {
            Transform root = transform.root;
            playerControls = root.GetComponent<PlayerControls>();
        }

        if (playerControls == null)
        {
            Debug.LogWarning($"[BirdAnimationController] PlayerControls component not found for {gameObject.name} - waiting for SetPlayerControls()", this);
            enabled = false;
        }
    }

    public void SetPlayerControls(PlayerControls controls)
    {
        playerControls = controls;

        if (playerControls == null)
        {
            Debug.LogError($"[BirdAnimationController] PlayerControls reference is null on {gameObject.name}", this);
            enabled = false;
        }
        else
        {
            enabled = true;
            Debug.Log($"[BirdAnimationController] PlayerControls successfully set for {gameObject.name}", this);
        }
    }

    private void Update()
    {
        if (playerControls == null || animator == null)
            return;

        UpdateWalkAnimation();
        UpdateFlyAnimation();
    }

    private void UpdateWalkAnimation()
    {
        bool isWalking = playerControls.walkingAnim;

        if (isWalking != wasWalking)
        {
            animator.SetBool(IsWalkingHash, isWalking);
            wasWalking = isWalking;
        }
    }

    private void UpdateFlyAnimation()
    {
        bool isFlying = playerControls.jumpingAnim;

        if (isFlying != wasFlying)
        {
            animator.SetBool(IsFlyingHash, isFlying);
            wasFlying = isFlying;
        }
    }

    // private void UpdateWiggleAnimation()
    // {
    //     bool isWiggling = playerControls.isWiggling; // Add this property to PlayerControls
    //     animator.SetBool(wiggleParameterName, isWiggling);
    // }
}