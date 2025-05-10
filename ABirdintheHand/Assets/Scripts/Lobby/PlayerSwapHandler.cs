using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Linq;

public class PlayerSwapHandler : MonoBehaviour
{
    public GameObject birdPrefab;
    public GameObject humanPrefab;

    private bool isInSwapZone = false;
    private PlayerInput playerInput;
    private Transform spawnTransform;

    private void Awake()
    {
        playerInput = GetComponentInChildren<PlayerInput>();
    }

    private void OnEnable()
    {
        playerInput.actions["Interact"].performed += OnInteract;
    }

    private void OnDisable()
    {
        playerInput.actions["Interact"].performed -= OnInteract;
    }

    public void EnterSwapZone(Transform swapZoneTransform)
    {
        isInSwapZone = true;
        spawnTransform = swapZoneTransform;
    }

    public void ExitSwapZone()
    {
        isInSwapZone = false;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!isInSwapZone) return;

        var inputManager = PlayerInputManager.instance;

        // Decide new prefab
        bool isCurrentlyBird = GetComponentInChildren<Cinemachine.CinemachineFreeLook>() != null;
        GameObject newPrefab = isCurrentlyBird ? humanPrefab : birdPrefab;

        // Change the default prefab temporarily
        inputManager.playerPrefab = newPrefab;

        // Save data
        var device = playerInput.devices[0];
        var index = playerInput.playerIndex;
        Vector3 spawnPos = transform.position;
        Quaternion spawnRot = transform.rotation;

        // Remove current player
        Destroy(gameObject);

        // Rejoin with same device
        inputManager.JoinPlayer(index, -1, null, device);

        // Optionally: reposition new player
        StartCoroutine(RepositionNextFrame(spawnPos, spawnRot));
    }

    private System.Collections.IEnumerator RepositionNextFrame(Vector3 position, Quaternion rotation)
    {
        yield return null; // wait one frame
        var newPlayer = GameObject.FindGameObjectsWithTag("Player")
            .FirstOrDefault(p => p.GetComponent<PlayerInput>()?.playerIndex == playerInput.playerIndex);

        if (newPlayer != null)
        {
            newPlayer.transform.position = position;
            newPlayer.transform.rotation = rotation;
        }
    }
}
