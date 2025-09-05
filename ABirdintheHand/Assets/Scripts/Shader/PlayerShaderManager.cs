using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShaderManager : MonoBehaviour
{
    private PlayerInputManager playerInputManager;
    private CamIDSetter cameraIDSetter;

    private void Awake()
    {
        cameraIDSetter = GetComponent<CamIDSetter>();
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        playerInputManager.onPlayerJoined += OnPlayerJoined;
    }

    private void OnPlayerJoined(PlayerInput player)
    {
        Camera playerCamera = player.transform.parent.GetComponentInChildren<Camera>();
        
        if (playerCamera != null && playerCamera.GetComponent<CamIDSetter>() == null)
        {
            playerCamera.gameObject.AddComponent<CamIDSetter>();
        }

        // Set player ID for ShaderSwapper
        ShaderSwapper shaderSwapper = player.GetComponent<ShaderSwapper>();
        if (shaderSwapper != null)
        {
            shaderSwapper.playerID = player.playerIndex + 1; // playerIndex is 0-based, so add 1
        }

       
    }
}