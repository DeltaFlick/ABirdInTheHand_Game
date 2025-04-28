using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.SceneManagement;

public class SceneInitialization : MonoBehaviour
{
    private void OnEnable()
    {
        SceneManager.activeSceneChanged += SceneBeginCheck;
    }


    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= SceneBeginCheck;
    }


    private void SceneBeginCheck(Scene fromScene, Scene toScene)
    {

        //foreach (var item in PlayerObjectHandler.playerControlSchemes)
        //{
        //    UnityEngine.Debug.Log(item);
        //}


        if (PlayerObjectHandler.shouldSpawnSelectedPlayers)
        {
            SpawnSelectedPlayers();
            PlayerObjectHandler.shouldSpawnSelectedPlayers = false;
        }
    }



    private void SpawnSelectedPlayers()
    {
        foreach (var player in PlayerObjectHandler.playerControllers)
        {
            var playerController = PlayerObjectHandler.playerControllers[player.Key];
            var playerObjectName = PlayerObjectHandler.playerSelectionNames[player.Key];
            var playerControlScheme = PlayerObjectHandler.playerControlSchemes[player.Key];

            GameObject parentPlayerObject = new GameObject();

            for (int i = 0; i < playerObjectName.Count; i++)
            {
                var currentObject = Resources.Load<GameObject>(playerObjectName[i]);

                if (i == 0)
                {
                    parentPlayerObject = currentObject;
                    PlayerInput playerInput = PlayerInput.Instantiate(currentObject, player.Key, playerControlScheme, -1, playerController);

                    currentObject.GetComponent<PlayerControls>().SetPlayerInputActive(true, playerInput);

                    var inputUser = playerInput.user;
                    playerInput.SwitchCurrentControlScheme(playerControlScheme);
                    InputUser.PerformPairingWithDevice(playerController, inputUser, InputUserPairingOptions.UnpairCurrentDevicesFromUser);
                }

                else
                {
                    Instantiate(currentObject, parentPlayerObject.transform);
                }



            }
        }
    }


}
