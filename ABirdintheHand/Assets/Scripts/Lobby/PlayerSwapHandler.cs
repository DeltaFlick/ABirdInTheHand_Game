using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

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
        var inputManager = PlayerInputManager.instance;

        bool isCurrentlyBird = GetComponentInChildren<Cinemachine.CinemachineFreeLook>() != null;
        GameObject newPrefab = isCurrentlyBird ? humanPrefab : birdPrefab;


        inputManager.playerPrefab = newPrefab;

        Destroy(gameObject);

        inputManager.JoinPlayer(playerIndex, -1, null, inputDevice);

        PlayerSwapCoordinator.Instance.RepositionPlayerNextFrame(playerIndex, teleportTarget);
        Destroy(gameObject);
    }
}

