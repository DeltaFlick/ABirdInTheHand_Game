using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.HighDefinition;

public class PlayerShaderManager : MonoBehaviour
{
    [SerializeField] private CustomPassVolume[] customPassVolumes = new CustomPassVolume[4]; // Direct references to volumes for players 1-4

    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        playerInputManager.onPlayerJoined += OnPlayerJoined;
        playerInputManager.onPlayerLeft += OnPlayerLeft;
    }

    public void OnPlayerJoined(PlayerInput player)
    {
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        int playerIndex = player.playerIndex;
        Debug.Log($"Player Index: {playerIndex}, Camera: {playerCamera}");

        //if (playerIndex < customPassVolumes.Length && customPassVolumes[playerIndex] != null)
        //{
        //    CustomPassVolume volume = customPassVolumes[playerIndex];
            
        //    volume.enabled = true;
        //    volume.targetCamera = playerCamera;

        //    ShaderSwapper shaderSwapper = player.GetComponent<ShaderSwapper>();
        //    if (shaderSwapper != null)
        //    {
        //        shaderSwapper.playerID = playerIndex + 1;
        //        Debug.Log(playerIndex);
        //        shaderSwapper.SetCustomPassVolume(volume);
        //    }
        //}
    }

    private void OnPlayerLeft(PlayerInput player)
    {
        int playerIndex = player.playerIndex;
        
        if (playerIndex < customPassVolumes.Length && customPassVolumes[playerIndex] != null)
        {
            customPassVolumes[playerIndex].enabled = false;
        }

        ShaderSwapper shaderSwapper = player.GetComponent<ShaderSwapper>();
        if (shaderSwapper != null)
        {
            shaderSwapper.RevertShader();
        }
    }
}