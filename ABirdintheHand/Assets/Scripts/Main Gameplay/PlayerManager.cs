using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private Dictionary<int, PlayerInput> players = new Dictionary<int, PlayerInput>();

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
        int index = player.playerIndex;

        if (players.ContainsKey(index))
        {
            if (players[index] != null)
            {
                Destroy(players[index].gameObject);
            }
            players[index] = player;
        }
        else
        {
            players.Add(index, player);
        }

        Transform playerParent = player.transform.parent;

        if (index < startingPoints.Count)
        {
            playerParent.position = startingPoints[index].position;
            playerParent.rotation = startingPoints[index].rotation;
        }
        else
        {
            playerParent.position = Vector3.zero;
            playerParent.rotation = Quaternion.identity;
        }

        if (index < playerLayers.Count)
        {
            int layerToAdd = (int)Mathf.Log(playerLayers[index].value, 2);

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
        }
        var inputHandler = playerParent.GetComponentInChildren<InputHandler>();
        if (inputHandler != null)
        {
            inputHandler.horizontal = player.actions.FindAction("Look");
        }
    }
}
