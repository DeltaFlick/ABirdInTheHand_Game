using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerObjectHandler : MonoBehaviour
{
   
    public static Dictionary<int, InputDevice> playerControllers = new Dictionary<int, InputDevice>();

    public static Dictionary<int, List<string>> playerSelectionNames = new Dictionary<int, List<string>>();
    public static Dictionary<int, string> playerControlSchemes = new Dictionary<int, string>();

    public static GameObject[] playerCursors;

    [SerializeField] public static bool shouldSpawnSelectedPlayers = false;
    [SerializeField] public static bool shouldPersistCursors = false;


    private void OnEnable()
    {
        CursorBehavior.DoneSelectingEvent += PlayersDoneSelecting;
    }

    private void OnDisable()
    {
        CursorBehavior.DoneSelectingEvent -= PlayersDoneSelecting;
    }




    private void PlayersDoneSelecting(object sender, EventArgs e)
    {
        // UnityEngine.Debug.Log("Player done method tripped");


        playerCursors = GameObject.FindGameObjectsWithTag("PlayerCursor");

        foreach (var cursor in playerCursors)
        {
            if (!cursor.GetComponent<CursorBehavior>().objectSelected)
            {
                UnityEngine.Debug.Log(cursor + " object has not selected a player!");
                return;
            }
        }



        for (int i = 0; i < playerCursors.Length; i++)
        {
            var playerInputComponent = playerCursors[i].GetComponent<PlayerInput>();
            var playerSelection = playerCursors[i].GetComponent<CursorBehavior>().playerSelection.name;

            var playerIndex = playerInputComponent.playerIndex;


            playerControllers.Add(playerIndex, playerInputComponent.devices[0]);

            if (!playerSelectionNames.ContainsKey(playerIndex))
            {
                playerSelectionNames.Add(playerIndex, new List<string>() { playerSelection });
            }
            else
            {
                var currentGameObjectList = playerSelectionNames[playerIndex];
                currentGameObjectList.Add(playerSelection);
            }

            playerControlSchemes.Add(playerInputComponent.playerIndex, playerInputComponent.currentControlScheme);
        }


        shouldSpawnSelectedPlayers = true;

        shouldPersistCursors = false;

        SceneManager.LoadScene("MainLevel");
    }
}