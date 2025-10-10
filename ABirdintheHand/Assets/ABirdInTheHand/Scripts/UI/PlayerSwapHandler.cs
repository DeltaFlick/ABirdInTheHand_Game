using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/// <Summary>
/// Handle player swapping between prefabs
/// </Summary>

public class PlayerSwapHandler : MonoBehaviour
{
    [Header("Alternate Character Prefabs")]
    [SerializeField] private GameObject birdPrefab;
    [SerializeField] private GameObject humanPrefab;

    private PlayerInput playerInput;
    private InputDevice inputDevice;
    private PlayerManager playerManager;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput not found on prefab root!");
            return;
        }

        inputDevice = playerInput.devices.FirstOrDefault();
        if (inputDevice == null)
        {
            inputDevice = playerInput.currentControlScheme == "Keyboard&Mouse"
                ? Keyboard.current
                : Gamepad.all.ElementAtOrDefault(playerInput.playerIndex);
        }

        playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager == null)
            Debug.LogError("No PlayerManager found in scene!");
    }

    public void Initialize(PlayerInput pi, InputDevice device, PlayerManager pm)
    {
        playerInput = pi ?? playerInput;
        inputDevice = device ?? inputDevice;
        playerManager = pm ?? playerManager;
    }

    private bool IsCurrentlyHuman()
    {
        return GetComponent<BirdIdentifier>() == null;
    }

    public void SwapToHuman()
    {
        if (IsCurrentlyHuman()) return;
        Swap(humanPrefab);
    }

    public void SwapToBird()
    {
        if (!IsCurrentlyHuman()) return;
        Swap(birdPrefab);
    }

    private void Swap(GameObject newPrefab)
    {
        if (newPrefab == null)
        {
            Debug.LogError("Swap failed: prefab is null!");
            return;
        }

        if (playerInput == null)
        {
            Debug.LogError("Swap failed: missing playerInput reference on old player.");
            return;
        }

        Vector3 oldPosition = transform.position;
        Quaternion oldRootRotation = transform.rotation;

        Camera oldCam = GetComponentInChildren<Camera>(true);
        Quaternion? oldCameraRotation = oldCam != null ? (Quaternion?)oldCam.transform.rotation : null;

        CameraController oldHumanCam = GetComponentInChildren<CameraController>(true);
        Quaternion? oldHumanOrientationRotation = null;
        if (oldHumanCam != null && oldHumanCam.orientation != null)
            oldHumanOrientationRotation = oldHumanCam.orientation.rotation;

        Rigidbody oldRb = GetComponent<Rigidbody>();
        Vector3? oldVelocity = oldRb != null ? (Vector3?)oldRb.velocity : null;
        Vector3? oldAngularVelocity = oldRb != null ? (Vector3?)oldRb.angularVelocity : null;

        ForceDrop.RequestDropAll();

        int index = playerInput.playerIndex;

        var device = inputDevice ?? playerInput.devices.FirstOrDefault();

        Destroy(gameObject);

        PlayerInput newPlayerInput = PlayerInput.Instantiate(
            newPrefab,
            index,
            controlScheme: null,
            pairWithDevice: device
        );

        if (newPlayerInput == null)
        {
            Debug.LogError("PlayerInput.Instantiate returned null!");
            return;
        }

        GameObject newPlayer = newPlayerInput.gameObject;

        newPlayer.transform.SetPositionAndRotation(oldPosition, oldRootRotation);

        if (oldCameraRotation.HasValue || oldHumanOrientationRotation.HasValue)
        {
            if (oldCameraRotation.HasValue)
            {
                Camera newCam = newPlayer.GetComponentInChildren<Camera>(true);
                if (newCam != null)
                    newCam.transform.rotation = oldCameraRotation.Value;
            }

            if (oldHumanOrientationRotation.HasValue)
            {
                CameraController newHumanCam = newPlayer.GetComponentInChildren<CameraController>(true);
                if (newHumanCam != null)
                {
                    Quaternion camRot = oldCameraRotation ?? newPlayer.GetComponentInChildren<Camera>(true)?.transform.rotation ?? Quaternion.identity;
                    newHumanCam.SetImmediateOrientation(camRot, oldHumanOrientationRotation.Value);
                }
            }
        }

        var helper = newPlayer.AddComponent<SwapPositionHelper>();
        helper.StartPositionFix(oldPosition, oldRootRotation, oldCameraRotation, oldHumanOrientationRotation);

        if (playerManager != null)
        {
            playerManager.ReplacePlayer(index, newPlayerInput);
            playerManager.SetupPlayer(newPlayerInput, useSpawnPoint: false);
        }

        PlayerSwapHandler newSwapHandler = newPlayer.GetComponent<PlayerSwapHandler>();
        if (newSwapHandler != null)
        {
            newSwapHandler.Initialize(newPlayerInput, device, playerManager);
        }

        PlayerMenuController uiController = FindObjectOfType<PlayerMenuController>();
        if (uiController != null && newSwapHandler != null)
        {
            uiController.PlayerSwapHandler = newSwapHandler;
        }
    }

    private IEnumerator ApplyVelocityNextFrame(Rigidbody rb, Vector3 velocity, Vector3 angular)
    {
        yield return null;
        rb.velocity = velocity;
        rb.angularVelocity = angular;
    }
}

public class SwapPositionHelper : MonoBehaviour
{
    private Vector3 pos;
    private Quaternion rot;
    private Quaternion? camRot;
    private Quaternion? humanOrientationRot;
    private bool started = false;

    public void StartPositionFix(Vector3 p, Quaternion r, Quaternion? cameraRotation = null, Quaternion? humanOriRot = null)
    {
        pos = p;
        rot = r;
        camRot = cameraRotation;
        humanOrientationRot = humanOriRot;

        if (!started)
        {
            started = true;
            StartCoroutine(FixNextFrame());
        }
    }

    private IEnumerator FixNextFrame()
    {
        yield return null;

        transform.SetPositionAndRotation(pos, rot);

        if (camRot.HasValue)
        {
            Camera cam = GetComponentInChildren<Camera>(true);
            if (cam != null)
                cam.transform.rotation = camRot.Value;
        }

        if (humanOrientationRot.HasValue)
        {
            CameraController hc = GetComponentInChildren<CameraController>(true);
            if (hc != null)
            {
                Quaternion camRotation = camRot ?? GetComponentInChildren<Camera>(true)?.transform.rotation ?? Quaternion.identity;
                hc.SetImmediateOrientation(camRotation, humanOrientationRot.Value);
            }
        }

        Destroy(this);
    }
}
