using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Users;

/// <summary>
/// Handles swapping player prefabs in multiplayer while preserving UI, InputSystem, and player state.
/// Supports multiple character prefabs in a single list.
/// </summary>

public class PlayerSwapHandler : MonoBehaviour
{
    [Header("Character Prefabs")]
    [SerializeField] private List<GameObject> characterPrefabs;

    private PlayerInput playerInput;
    private PlayerManager playerManager;
    private InputDevice inputDevice;
    private GameObject currentPrefab;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerManager = FindObjectOfType<PlayerManager>();

        inputDevice = playerInput.devices.FirstOrDefault();
        if (inputDevice == null)
        {
            inputDevice = playerInput.currentControlScheme == "Keyboard&Mouse"
                ? Keyboard.current
                : Gamepad.all.ElementAtOrDefault(playerInput.playerIndex);
        }

        currentPrefab = gameObject;

        EnableUIActionMap();

        Debug.Log($"[PlayerSwapHandler] Initialised for {playerInput.gameObject.name} using {inputDevice?.displayName ?? "None"}");
    }

    public void SwapToCharacter(int index)
    {
        if (index < 0 || index >= characterPrefabs.Count) return;
        Swap(characterPrefabs[index]);
    }

    public void SwapToCharacter(string characterName)
    {
        var prefab = characterPrefabs.FirstOrDefault(p => p.name == characterName);
        if (prefab != null) Swap(prefab);
    }

    private void Swap(GameObject newPrefab)
    {
        if (newPrefab == null || playerInput == null || newPrefab == currentPrefab) return;
        currentPrefab = newPrefab;

        Vector3 oldPosition = transform.position;
        Quaternion oldRotation = transform.rotation;

        Camera oldCam = GetComponentInChildren<Camera>(true);
        Quaternion? oldCamRot = oldCam != null ? (Quaternion?)oldCam.transform.rotation : null;

        CameraController oldCamController = GetComponentInChildren<CameraController>(true);
        Quaternion? oldCamOrientation = oldCamController != null && oldCamController.orientation != null
            ? (Quaternion?)oldCamController.orientation.rotation
            : null;

        Rigidbody oldRb = GetComponent<Rigidbody>();
        Vector3? oldVelocity = oldRb != null ? (Vector3?)oldRb.velocity : null;
        Vector3? oldAngularVelocity = oldRb != null ? (Vector3?)oldRb.angularVelocity : null;

        int index = playerInput.playerIndex;
        var device = inputDevice ?? playerInput.devices.FirstOrDefault();

        Destroy(gameObject);

        PlayerInput newPlayerInput = PlayerInput.Instantiate(
            newPrefab,
            index,
            controlScheme: null,
            pairWithDevice: device
        );

        if (newPlayerInput == null) return;

        GameObject newPlayer = newPlayerInput.gameObject;
        newPlayer.transform.SetPositionAndRotation(oldPosition, oldRotation);

        if (oldCamRot.HasValue)
        {
            Camera newCam = newPlayer.GetComponentInChildren<Camera>(true);
            if (newCam != null) newCam.transform.rotation = oldCamRot.Value;
        }

        if (oldCamOrientation.HasValue)
        {
            CameraController newCamController = newPlayer.GetComponentInChildren<CameraController>(true);
            if (newCamController != null)
            {
                Quaternion camRot = oldCamRot ?? newPlayer.GetComponentInChildren<Camera>(true)?.transform.rotation ?? Quaternion.identity;
                newCamController.SetImmediateOrientation(camRot, oldCamOrientation.Value);
            }
        }

        if (oldRb != null && oldVelocity.HasValue && oldAngularVelocity.HasValue)
        {
            Rigidbody newRb = newPlayer.GetComponent<Rigidbody>();
            if (newRb != null)
                StartCoroutine(ApplyVelocityNextFrame(newRb, oldVelocity.Value, oldAngularVelocity.Value));
        }

        if (playerManager != null)
        {
            playerManager.ReplacePlayer(index, newPlayerInput);
            playerManager.SetupPlayer(newPlayerInput, useSpawnPoint: false);
        }

        PlayerSwapHandler newSwapHandler = newPlayer.GetComponent<PlayerSwapHandler>();
        if (newSwapHandler != null)
        {
            newSwapHandler.Initialise(newPlayerInput, device, playerManager);
        }

        InputSystemUIInputModule newUiModule = newPlayer.GetComponentInChildren<InputSystemUIInputModule>(true);
        MultiplayerEventSystem newEventSystem = newPlayer.GetComponentInChildren<MultiplayerEventSystem>(true);

        if (newUiModule != null && newEventSystem != null)
        {
            newUiModule.actionsAsset = newPlayerInput.actions;
            newUiModule.move = InputActionReference.Create(newPlayerInput.actions["UI/Move"]);
            newUiModule.submit = InputActionReference.Create(newPlayerInput.actions["UI/Submit"]);
            newUiModule.cancel = InputActionReference.Create(newPlayerInput.actions["UI/Cancel"]);

            var user = newPlayerInput.user;
            if (user.valid)
            {
                InputUser.PerformPairingWithDevice(device, user);
                newEventSystem.playerRoot = newPlayer;
                newEventSystem.SetSelectedGameObject(null);
            }
        }
        EnableUIActionMapForAllPlayers();

        Debug.Log($"[PlayerSwapHandler] Swap complete for Player {index} ({newPlayer.name}).");
    }

    private IEnumerator ApplyVelocityNextFrame(Rigidbody rb, Vector3 velocity, Vector3 angular)
    {
        yield return null;
        rb.velocity = velocity;
        rb.angularVelocity = angular;
    }

    private void EnableUIActionMap()
    {
        playerInput?.actions.FindActionMap("UI")?.Enable();
    }

    private void EnableUIActionMapForAllPlayers()
    {
        foreach (var pi in PlayerInput.all)
        {
            var uiMap = pi.actions.FindActionMap("UI");
            if (uiMap != null && !uiMap.enabled)
                uiMap.Enable();
        }
    }

    public void Initialise(PlayerInput pi, InputDevice device, PlayerManager pm)
    {
        playerInput = pi ?? playerInput;
        inputDevice = device ?? inputDevice;
        playerManager = pm ?? playerManager;
        EnableUIActionMap();
    }
}
