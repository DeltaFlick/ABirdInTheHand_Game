using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwapHandler : MonoBehaviour
{
    [Header("Alternate Character Prefabs")]
    public GameObject birdPrefab;
    public GameObject humanPrefab;

    private bool isInSwapZone = false;
    private Transform teleportTarget;
    private PlayerInput playerInput;
    private int playerIndex;
    private InputDevice inputDevice;

    private void Awake()
    {
        playerInput = GetComponentInChildren<PlayerInput>();
        playerIndex = playerInput.playerIndex;

        inputDevice = playerInput.devices.FirstOrDefault();

        if (inputDevice == null)
        {
            inputDevice = playerInput.currentControlScheme == "Keyboard&Mouse"
                ? Keyboard.current
                : Gamepad.all.ElementAtOrDefault(playerIndex);
        }
    }

    private void OnEnable()
    {
        playerInput.actions["Interact"].performed += OnInteract;
    }

    private void OnDisable()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            playerInput.actions["Interact"].performed -= OnInteract;
        }
    }

    public void EnterSwapZone(Transform target)
    {
        isInSwapZone = true;
        teleportTarget = target;
    }

    public void ExitSwapZone()
    {
        isInSwapZone = false;
        teleportTarget = null;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!isInSwapZone || teleportTarget == null)
        {
            Debug.Log("Not in swap zone. Ignoring swap.");
            return;
        }

        bool isCurrentlyBird = GetComponentInChildren<BirdIdentifier>() != null;
        GameObject newPrefab = isCurrentlyBird ? humanPrefab : birdPrefab;

        int index = playerInput.playerIndex;
        var device = inputDevice;

        Destroy(gameObject);

        PlayerInput newPlayerInput = PlayerInput.Instantiate(newPrefab, index, controlScheme: null, pairWithDevice: device);
        GameObject newPlayer = newPlayerInput.gameObject;

        newPlayer.transform.position = teleportTarget.position;
        newPlayer.transform.rotation = teleportTarget.rotation;

        PlayerSwapCoordinator.Instance.RepositionPlayerNextFrame(index, teleportTarget);
    }
}