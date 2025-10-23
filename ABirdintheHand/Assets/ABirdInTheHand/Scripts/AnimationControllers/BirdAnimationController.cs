using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//should be useable for the human aswell. clean up later
// fly = jump

public class BirdAnimationController : MonoBehaviour
{
    private Animator animator;
    private PlayerControls playerControls;

    private string walkParameterName = "IsWalking";
    private string flyParameterName = "IsFlying";
    private string wiggleParameterName = "IsWiggling";


    void Start()
    {
        animator = GetComponent<Animator>();
        playerControls = GetComponentInParent<PlayerControls>();
        if (playerControls == null)
        {
            Debug.LogError("PlayerControls component not found on Bird.");
        }
    }

    void Update()
    {
        if (playerControls != null)
        {
            UpdateWalkAnimation();
            UpdateFlyAnimation();
            //  UpdateWiggleAnimation();
        }
    }

    private void UpdateWalkAnimation()
    {
        bool isWalking = playerControls.walkingAnim;
        animator.SetBool(walkParameterName, isWalking);
    }

    private void UpdateFlyAnimation()
    {
        bool isFlying = playerControls.jumpingAnim;
        animator.SetBool(flyParameterName, isFlying);
    }

    // private void UpdateWiggleAnimation()
    // {
    //     bool isWiggling = playerControls.isWiggling; // Add this property to PlayerControls
    //     animator.SetBool(wiggleParameterName, isWiggling);
    // }

}
