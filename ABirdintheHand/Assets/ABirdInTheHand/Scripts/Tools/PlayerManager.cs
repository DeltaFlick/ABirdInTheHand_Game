using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

/// <summary>
/// Dynamically manages player joining, spawning, and layer setup
/// </summary>
public class PlayerManager : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private List<Transform> startingPoints;
    [SerializeField] public List<LayerMask> playerLayers;

    private Dictionary<int, PlayerInput> players = new Dictionary<int, PlayerInput>();
    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();

        if (playerInputManager == null)
        {
            Debug.LogError("[PlayerManager] No PlayerInputManager found in scene!", this);
        }
    }

    private void OnEnable()
    {
        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined += AddPlayer;
        }
    }

    private void OnDisable()
    {
        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined -= AddPlayer;
        }
    }

    public void AddPlayer(PlayerInput player)
    {
        if (player == null)
        {
            Debug.LogWarning("[PlayerManager] Attempted to add null player!", this);
            return;
        }

        int index = player.playerIndex;
        players[index] = player;

        SetupPlayer(player, useSpawnPoint: true);

        Debug.Log($"[PlayerManager] Player {index} added and setup", this);
    }

    public void ReplacePlayer(int index, PlayerInput newPlayer)
    {
        if (newPlayer == null)
        {
            Debug.LogWarning($"[PlayerManager] Attempted to replace player {index} with null!", this);
            return;
        }

        if (players.ContainsKey(index))
        {
            players[index] = newPlayer;
        }
        else
        {
            players.Add(index, newPlayer);
        }

        Debug.Log($"[PlayerManager] Player {index} replaced", this);
    }

    public void SetupPlayer(PlayerInput player, bool useSpawnPoint = true)
    {
        if (player == null)
        {
            Debug.LogWarning("[PlayerManager] Attempted to setup null player!", this);
            return;
        }

        Transform playerTransform = player.transform;
        int index = player.playerIndex;

        if (useSpawnPoint)
        {
            SetupPlayerSpawn(playerTransform, index);
        }

        if (index < playerLayers.Count)
        {
            SetupPlayerLayers(playerTransform, index);
        }
        else
        {
            Debug.LogWarning($"[PlayerManager] Player index {index} exceeds playerLayers list size", this);
        }

        SetupInputHandler(playerTransform, player);
    }

    private void SetupPlayerSpawn(Transform playerTransform, int index)
    {
        if (index >= startingPoints.Count || startingPoints[index] == null)
        {
            Debug.LogWarning($"[PlayerManager] No spawn point for player {index}", this);
            return;
        }

        Transform spawnPoint = startingPoints[index];
        Rigidbody rb = playerTransform.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.position = spawnPoint.position;
            rb.rotation = spawnPoint.rotation;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            playerTransform.position = spawnPoint.position;
            playerTransform.rotation = spawnPoint.rotation;

            Physics.SyncTransforms();
        }
        else
        {
            playerTransform.position = spawnPoint.position;
            playerTransform.rotation = spawnPoint.rotation;
        }
    }

    private void SetupPlayerLayers(Transform playerTransform, int index)
    {
        int ownLayer = (int)Mathf.Log(playerLayers[index].value, 2);

        Renderer[] renderers = playerTransform.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            renderer.gameObject.layer = ownLayer;
        }

        Renderer rootRenderer = playerTransform.GetComponent<Renderer>();
        if (rootRenderer != null)
        {
            rootRenderer.gameObject.layer = ownLayer;
        }

        Camera cam = GetFromRootOrChildren<Camera>(playerTransform);
        if (cam != null)
        {
            cam.cullingMask = -1;
            cam.cullingMask &= ~(1 << ownLayer);
        }

        CinemachineFreeLook freeLook = GetFromRootOrChildren<CinemachineFreeLook>(playerTransform);
        if (freeLook != null)
        {
            freeLook.gameObject.layer = LayerMask.NameToLayer("Default");
        }
    }

    private void SetupInputHandler(Transform playerTransform, PlayerInput player)
    {
        InputHandler inputHandler = GetFromRootOrChildren<InputHandler>(playerTransform);

        if (inputHandler != null)
        {
            InputAction lookAction = player.actions.FindAction("Look");
            if (lookAction != null)
            {
                inputHandler.horizontal = lookAction;
            }
        }
    }

    private T GetFromRootOrChildren<T>(Transform t) where T : Component
    {
        T comp = t.GetComponent<T>();

        if (comp == null)
        {
            comp = t.GetComponentInChildren<T>(true);
        }

        return comp;
    }

    public PlayerInput GetPlayer(int index)
    {
        return players.ContainsKey(index) ? players[index] : null;
    }

    public int GetPlayerCount()
    {
        return players.Count;
    }

    private void OnDestroy()
    {
        if (playerInputManager != null)
        {
            playerInputManager.onPlayerJoined -= AddPlayer;
        }

        players.Clear();
    }
}