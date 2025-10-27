using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

/// <Summary>
/// Dynamically add and index players
/// </Summary>

public class PlayerManager : MonoBehaviour
{
    private Dictionary<int, PlayerInput> players = new Dictionary<int, PlayerInput>();
    [Header("Player Setup")]
    [SerializeField] private List<Transform> startingPoints;
    [SerializeField] public List<LayerMask> playerLayers;
    private PlayerInputManager playerInputManager;
    private void Awake()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        if (playerInputManager == null)
            Debug.LogError("No PlayerInputManager found in scene!");
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
        if (player == null) return;
        int index = player.playerIndex;
        players[index] = player;
        SetupPlayer(player, useSpawnPoint: true);
    }
    public void ReplacePlayer(int index, PlayerInput newPlayer)
    {
        if (players.ContainsKey(index))
        {
            players[index] = newPlayer;
        }
        else
        {
            players.Add(index, newPlayer);
        }
    }
    public void SetupPlayer(PlayerInput player, bool useSpawnPoint = true)
    {
        if (player == null) return;
        Transform playerTransform = player.transform;
        int index = player.playerIndex;

        if (useSpawnPoint && index < startingPoints.Count && startingPoints[index] != null)
        {
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.position = startingPoints[index].position;
                rb.rotation = startingPoints[index].rotation;

                playerTransform.position = startingPoints[index].position;
                playerTransform.rotation = startingPoints[index].rotation;

                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                Physics.SyncTransforms();
            }
            else
            {
                playerTransform.position = startingPoints[index].position;
                playerTransform.rotation = startingPoints[index].rotation;
            }
        }

        if (index < playerLayers.Count)
        {
            int ownLayer = (int)Mathf.Log(playerLayers[index].value, 2);
            foreach (Renderer renderer in playerTransform.GetComponentsInChildren<Renderer>(true))
                renderer.gameObject.layer = ownLayer;
            Renderer rootRenderer = playerTransform.GetComponent<Renderer>();
            if (rootRenderer != null)
                rootRenderer.gameObject.layer = ownLayer;
            Camera cam = GetFromRootOrChildren<Camera>(playerTransform);
            if (cam != null)
            {
                cam.cullingMask = -1;
                cam.cullingMask &= ~(1 << ownLayer);
            }
            CinemachineFreeLook freeLook = GetFromRootOrChildren<CinemachineFreeLook>(playerTransform);
            if (freeLook != null)
                freeLook.gameObject.layer = LayerMask.NameToLayer("Default");
        }
        InputHandler inputHandler = GetFromRootOrChildren<InputHandler>(playerTransform);
        if (inputHandler != null)
        {
            var lookAction = player.actions.FindAction("Look");
            if (lookAction != null)
                inputHandler.horizontal = lookAction;
        }
    }
    private T GetFromRootOrChildren<T>(Transform t) where T : Component
    {
        T comp = t.GetComponent<T>();
        if (comp == null)
            comp = t.GetComponentInChildren<T>(true);
        return comp;
    }
}