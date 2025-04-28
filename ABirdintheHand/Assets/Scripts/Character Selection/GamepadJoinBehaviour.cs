using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Users;
using UnityEngine.UI;

public class GamepadJoinBehaviour : MonoBehaviour
{
    [SerializeField] Canvas currentCanvas;
    public int numberOfActivePlayers { get; private set; } = 0;






    void Start()
    {
        var myAction = new InputAction(binding: "/*/<button>");
        myAction.performed += (action) =>
        {
            AddPlayer(action.control.device);
        };
        myAction.Enable();
    }






    void AddPlayer(InputDevice device)
    {
        foreach (var player in PlayerInput.all)
        {
            foreach (var playerDevice in player.devices)
            {
                if (device == playerDevice)
                {
                    return;
                }
            }
        }

        //UnityEngine.Debug.Log(device.device);


        if (!device.displayName.Contains("Controller") && !device.displayName.Contains("Joystick") && !device.displayName.Contains("Gamepad"))
            return;

        var playerNumberToAdd = PlayerInput.all.Count + 1;

        string controlScheme = "";

        if (device.displayName.Contains("Controller") || device.displayName.Contains("Gamepad"))
            controlScheme = "Gamepad";
        else if (device.displayName.Contains("Joystick"))
            controlScheme = "Joystick";

        GameObject playerCursor = Resources.Load<GameObject>($"CursorPrefabs/P{playerNumberToAdd}_Cursor");


        if (!playerCursor.activeInHierarchy)
        {
            PlayerInput theCursor = PlayerInput.Instantiate(playerCursor, -1, controlScheme, -1, device);
            theCursor.transform.parent = currentCanvas.transform;
            theCursor.transform.localScale = new Vector3(1f, 1f, 1f);
        }

    }



    public void OnPlayerJoin(PlayerInput input)
    {
        numberOfActivePlayers = PlayerInput.all.Count;
        UnityEngine.Debug.Log("There are currently " + numberOfActivePlayers + " players.");
    }

    public void OnPlayerLeft(PlayerInput input)
    {
        numberOfActivePlayers = PlayerInput.all.Count;
        UnityEngine.Debug.Log("There are currently " + numberOfActivePlayers + " players.");
    }

}