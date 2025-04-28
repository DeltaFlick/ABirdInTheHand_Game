using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    private Vector2 movement;
    public Rigidbody rb { get; private set; }
    public PlayerInput pi { get; private set; }


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pi = GetComponent<PlayerInput>();
        // UnityEngine.Debug.Log("Variable set at " + Time.time);
    }



    /// <summary>
    /// Useful to activate the PlayerInput component after instantiating the object in a new scene
    /// </summary>
    /// <param name="activation"></param>
    /// <param name="playerInput">Pass in the PlayerInput that was instantiated</param>
    public void SetPlayerInputActive(bool activation, PlayerInput playerInput)
    {
        // UnityEngine.Debug.Log("Activating pi at " + Time.time);
        if (pi == null)
            pi = playerInput;

        pi.enabled = activation;
    }



    void Update()
    {
        if (GetComponent<PlayerInput>().enabled)
            rb.AddForce(movement);

        // UnityEngine.Debug.Log(PlayerInput.all);
    }



    public void OnMove(InputAction.CallbackContext context)
    {
        movement = context.ReadValue<Vector2>() * 10;
    }
}