using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwapHandler : MonoBehaviour
{
    [Header("Alternate Character Prefabs")]
    public GameObject birdPrefab;
    public GameObject humanPrefab;

    private PlayerInput playerInput;
    private InputDevice inputDevice;

    private void Awake()
    {
        playerInput = GetComponentInChildren<PlayerInput>();
        inputDevice = playerInput.devices.FirstOrDefault();

        if (inputDevice == null)
        {
            inputDevice = playerInput.currentControlScheme == "Keyboard&Mouse"
                ? Keyboard.current
                : Gamepad.all.ElementAtOrDefault(playerInput.playerIndex);
        }
    }

    private bool IsCurrentlyHuman()
    {
        return GetComponentInChildren<BirdIdentifier>() == null;
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

        PickupManager.RequestDropAll();
        var interactHandler = GetComponentInChildren<InteractionHandler>();
        if (interactHandler != null)
        {
            var currentInteractableField = typeof(InteractionHandler)
                .GetField("currentInteractable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (currentInteractableField != null)
            {
                var current = currentInteractableField.GetValue(interactHandler) as IInteractable;
                current?.HidePrompt();
            }
        }

        Vector3 oldPosition = transform.position;
        Quaternion oldRotation = transform.rotation;
        int index = playerInput.playerIndex;
        var device = inputDevice;

        Destroy(gameObject);

        PlayerInput newPlayerInput = PlayerInput.Instantiate(newPrefab, index, controlScheme: null, pairWithDevice: device);
        GameObject newPlayer = newPlayerInput.gameObject;

        newPlayer.transform.SetPositionAndRotation(oldPosition, oldRotation);
    }
}


