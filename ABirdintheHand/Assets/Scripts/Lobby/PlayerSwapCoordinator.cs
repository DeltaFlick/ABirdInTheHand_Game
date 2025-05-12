using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSwapCoordinator : MonoBehaviour
{
    public static PlayerSwapCoordinator Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optional: uncomment this if you want the coordinator to persist between scenes
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RepositionPlayerNextFrame(int playerIndex, Transform target)
    {
        StartCoroutine(RepositionNextFrame(playerIndex, target));
    }

    private IEnumerator RepositionNextFrame(int playerIndex, Transform target)
    {
        yield return null; // Wait until the next frame

        var newPlayer = GameObject.FindGameObjectsWithTag("Player")
            .FirstOrDefault(p => p.GetComponent<PlayerInput>()?.playerIndex == playerIndex);

        if (newPlayer != null && target != null)
        {
            newPlayer.transform.position = target.position;
            newPlayer.transform.rotation = target.rotation;
        }
        else
        {
            Debug.LogWarning($"[PlayerSwapCoordinator] Reposition failed: newPlayer = {newPlayer}, target = {target}");
        }
    }
}
