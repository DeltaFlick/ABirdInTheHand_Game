using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles character swapping system for the Overlord controller
/// </summary>
public class OverlordSwapHandler : MonoBehaviour
{
    public event Action<GameObject> OnVisualChanged;

    [Header("Character Prefabs")]
    [SerializeField] private List<GameObject> characterPrefabs;

    [Header("References")]
    [SerializeField] private Transform visualsContainer;

    [Header("Initialization")]
    [SerializeField] private int defaultCharacterIndex = 0;

    private GameObject currentVisual;
    private PlayerInput playerInput;
    private Camera playerCamera;
    private int playerLayer;
    private PlayerMenuController menuController;

    public GameObject CurrentVisual => currentVisual;

    private void Awake()
    {
        if (visualsContainer == null)
        {
            visualsContainer = transform;
        }

        playerInput = GetComponent<PlayerInput>();
        playerCamera = GetComponentInChildren<Camera>(true);
        menuController = GetComponent<PlayerMenuController>();

        if (menuController == null)
        {
            Debug.LogWarning("[OverlordSwapHandler] No PlayerMenuController found on this overlord prefab!", this);
        }

        if (playerInput != null)
        {
            playerLayer = 9 + playerInput.playerIndex;
        }
        else
        {
            Debug.LogWarning("[OverlordSwapHandler] PlayerInput not found, using default layer 9", this);
            playerLayer = 9;
        }
    }

    private void Start()
    {
        StartCoroutine(InitializeAfterFrame());
    }

    private IEnumerator InitializeAfterFrame()
    {
        yield return null;

        if (characterPrefabs == null || characterPrefabs.Count == 0)
        {
            Debug.LogWarning("[OverlordSwapHandler] No character prefabs assigned!", this);
            yield break;
        }

        int idx = Mathf.Clamp(defaultCharacterIndex, 0, characterPrefabs.Count - 1);
        SwapToCharacter(idx);
    }

    public void SwapToCharacter(int index)
    {
        if (characterPrefabs == null || characterPrefabs.Count == 0)
        {
            Debug.LogWarning("[OverlordSwapHandler] No character prefabs available!", this);
            return;
        }

        if (index < 0 || index >= characterPrefabs.Count)
        {
            Debug.LogWarning($"[OverlordSwapHandler] Invalid character index: {index}", this);
            return;
        }

        Swap(characterPrefabs[index]);
    }

    public void SwapToCharacter(string characterName)
    {
        if (characterPrefabs == null || characterPrefabs.Count == 0)
        {
            Debug.LogWarning("[OverlordSwapHandler] No character prefabs available!", this);
            return;
        }

        GameObject prefab = characterPrefabs.Find(p => p != null && p.name == characterName);

        if (prefab != null)
        {
            Swap(prefab);
        }
        else
        {
            Debug.LogWarning($"[OverlordSwapHandler] Character '{characterName}' not found in prefabs list!", this);
        }
    }

    public GameObject GetPrefabByReference(GameObject prefab)
    {
        if (characterPrefabs == null || prefab == null)
            return null;

        return characterPrefabs.Find(p => p == prefab);
    }

    private void Swap(GameObject newPrefab)
    {
        if (newPrefab == null)
        {
            Debug.LogWarning("[OverlordSwapHandler] Attempted to swap to null prefab!", this);
            return;
        }

        if (currentVisual != null)
        {
            Destroy(currentVisual);
        }

        currentVisual = Instantiate(newPrefab);
        currentVisual.transform.SetParent(visualsContainer, worldPositionStays: false);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
        currentVisual.transform.localScale = Vector3.one;

        SetLayerRecursively(currentVisual, playerLayer);

        Rigidbody[] rigidbodies = currentVisual.GetComponentsInChildren<Rigidbody>(true);
        foreach (var rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        Collider[] colliders = currentVisual.GetComponentsInChildren<Collider>(true);
        foreach (var col in colliders)
        {
            col.isTrigger = true;
        }

        Animator[] animators = currentVisual.GetComponentsInChildren<Animator>(true);
        foreach (var anim in animators)
        {
            anim.applyRootMotion = false;
        }

        BirdIdentifier[] birds = currentVisual.GetComponentsInChildren<BirdIdentifier>(true);
        foreach (var bird in birds)
        {
            bird.MenuController = menuController;
        }

        PlayerControls playerControls = GetComponent<PlayerControls>();
        if (playerControls != null)
        {
            BirdAnimationController[] animControllers = currentVisual.GetComponentsInChildren<BirdAnimationController>(true);
            foreach (var animController in animControllers)
            {
                if (animController != null)
                {
                    // The BirdAnimationController already gets PlayerControls from parent
                    // So this may not be necessary (still testing)
                }
            }
        }
        else
        {
            Debug.LogWarning("[OverlordSwapHandler] PlayerControls component not found on overlord!", this);
        }

        if (playerCamera != null)
        {
            CameraHolder holder = currentVisual.GetComponentInChildren<CameraHolder>(true);

            if (holder != null && holder.cameraPosition != null)
            {
                playerCamera.transform.SetParent(holder.cameraPosition, worldPositionStays: false);
                playerCamera.transform.localPosition = Vector3.zero;
                playerCamera.transform.localRotation = Quaternion.identity;

                playerCamera.cullingMask &= ~(1 << playerLayer);
            }
            else
            {
                playerCamera.transform.SetParent(visualsContainer, worldPositionStays: false);
            }
        }

        FollowParentRoot follow = currentVisual.GetComponent<FollowParentRoot>();
        if (follow == null)
        {
            follow = currentVisual.AddComponent<FollowParentRoot>();
        }
        follow.SetTarget(visualsContainer);

        OnVisualChanged?.Invoke(currentVisual);

        if (playerInput != null)
        {
            Debug.Log($"[OverlordSwap] Player {playerInput.playerIndex} swapped to {newPrefab.name}", this);
        }
        else
        {
            Debug.Log($"[OverlordSwap] Swapped to {newPrefab.name} (playerInput null)", this);
        }
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;

        obj.layer = layer;

        Transform[] children = obj.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in children)
        {
            t.gameObject.layer = layer;
        }
    }

    private void OnDestroy()
    {
        if (currentVisual != null)
        {
            Destroy(currentVisual);
            currentVisual = null;
        }
    }
}