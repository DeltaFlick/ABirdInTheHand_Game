using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private List<PlayerInput> players = new List<PlayerInput>();
    [SerializeField]
    private List<Transform> startingPoints;
    [SerializeField]
    private List<LayerMask> playerLayers;

    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
    }

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= AddPlayer;
    }

    public void AddPlayer(PlayerInput player)
    {
        players.Add(player);

        Transform playerParent = player.transform.parent;
        playerParent.position = startingPoints[players.Count - 1].position;

        int layerToAdd = (int)Mathf.Log(playerLayers[players.Count - 1].value, 2);

        var freeLook = playerParent.GetComponentInChildren<CinemachineFreeLook>();
        if (freeLook != null)
        {
            freeLook.gameObject.layer = layerToAdd;
        }

        var cam = playerParent.GetComponentInChildren<Camera>();
        if (cam != null)
        {
            cam.cullingMask |= 1 << layerToAdd;
        }

        var inputHandler = playerParent.GetComponentInChildren<InputHandler>();
        if (inputHandler != null)
        {
            inputHandler.horizontal = player.actions.FindAction("Look");
        }
    }
}