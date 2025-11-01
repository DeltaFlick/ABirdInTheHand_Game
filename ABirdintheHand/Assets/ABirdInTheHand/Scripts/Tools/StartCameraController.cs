using UnityEngine;
using UnityEngine.InputSystem;

public class StartCameraController : MonoBehaviour
{
    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        Debug.Log("StartCameraController: Awake called");
        playerInputManager = FindObjectOfType<PlayerInputManager>();

        if (playerInputManager == null)
        {
            Debug.LogError("StartCameraController: PlayerInputManager NOT FOUND!");
        }
        else
        {
            Debug.Log("StartCameraController: PlayerInputManager found successfully");
        }
    }

    private void OnEnable()
    {
        Debug.Log("StartCameraController: OnEnable called");
        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined += ToggleThis;
            Debug.Log("StartCameraController: Subscribed to onPlayerJoined");
        }
    }

    private void OnDisable()
    {
        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined -= ToggleThis;
        }
    }

    private void ToggleThis(PlayerInput player)
    {
        Debug.Log("StartCameraController: Player joined! Disabling start camera");
        this.gameObject.SetActive(false);
    }
}