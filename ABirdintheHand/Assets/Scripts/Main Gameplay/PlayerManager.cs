using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private Dictionary<int, PlayerInput> players = new Dictionary<int, PlayerInput>();

    [Header("Player Setup")]
    [SerializeField] private List<Transform> startingPoints;
    [SerializeField] private List<LayerMask> playerLayers;

    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        if (playerInputManager == null)
        {
            Debug.LogError("No PlayerInputManager found in the scene!");
        }
    }

    private void OnEnable()
    {
        if (playerInputManager != null)
            playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable()
    {
        if (playerInputManager != null)
            playerInputManager.onPlayerJoined -= AddPlayer;
    }

    public void AddPlayer(PlayerInput player)
    {
        if (player == null)
        {
            Debug.LogError("AddPlayer called with null PlayerInput!");
            return;
        }

        int index = player.playerIndex;

        if (players.ContainsKey(index))
        {
            if (players[index] != null)
                Destroy(players[index].gameObject);

            players[index] = player;
        }
        else
        {
            players.Add(index, player);
        }

        Transform playerTransform = player.transform;

        if (index < startingPoints.Count && startingPoints[index] != null)
        {
            playerTransform.position = startingPoints[index].position;
            playerTransform.rotation = startingPoints[index].rotation;
        }
        else
        {
            playerTransform.position = Vector3.zero;
            playerTransform.rotation = Quaternion.identity;
        }

        // Layer assignment
        if (index < playerLayers.Count)
        {
            int ownLayer = (int)Mathf.Log(playerLayers[index].value, 2);

            foreach (Renderer renderer in playerTransform.GetComponentsInChildren<Renderer>())
            {
                renderer.gameObject.layer = ownLayer;
            }

            Camera cam = playerTransform.GetComponentInChildren<Camera>();
            if (cam != null)
            {
                cam.cullingMask = -1;
                cam.cullingMask &= ~(1 << ownLayer);
            }

            CinemachineFreeLook freeLook = playerTransform.GetComponentInChildren<CinemachineFreeLook>();
            if (freeLook != null)
            {
                freeLook.gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }

        InputHandler inputHandler = playerTransform.GetComponent<InputHandler>();
        if (inputHandler == null)
        {
            inputHandler = playerTransform.GetComponentInChildren<InputHandler>();
        }

        if (inputHandler != null)
        {
            var lookAction = player.actions.FindAction("Look");
            if (lookAction != null)
                inputHandler.horizontal = lookAction;
            else
                Debug.LogWarning($"Player {index} has no 'Look' action in InputActions.");
        }
        else
        {
            Debug.LogWarning($"Player {index} has no InputHandler component.");
        }
    }

    public Dictionary<int, PlayerInput> GetAllPlayers()
    {
        return players;
    }
}
