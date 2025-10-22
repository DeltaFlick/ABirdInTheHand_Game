using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    public GameObject CurrentVisual => currentVisual;

    private void Awake()
    {
        if (visualsContainer == null)
            visualsContainer = transform;

        playerInput = GetComponent<PlayerInput>();
        playerCamera = GetComponentInChildren<Camera>(true);

        playerLayer = 9 + playerInput.playerIndex;
    }

    private void Start()
    {
        StartCoroutine(InitializeAfterFrame());
    }

    private IEnumerator InitializeAfterFrame()
    {
        yield return null;

        if (characterPrefabs == null || characterPrefabs.Count == 0)
            yield break;

        int idx = Mathf.Clamp(defaultCharacterIndex, 0, characterPrefabs.Count - 1);
        SwapToCharacter(idx);
    }

    public void SwapToCharacter(int index)
    {
        if (characterPrefabs == null || characterPrefabs.Count == 0) return;
        if (index < 0 || index >= characterPrefabs.Count) return;
        Swap(characterPrefabs[index]);
    }

    public void SwapToCharacter(string characterName)
    {
        if (characterPrefabs == null || characterPrefabs.Count == 0) return;
        var prefab = characterPrefabs.Find(p => p != null && p.name == characterName);
        if (prefab != null) Swap(prefab);
    }

    private void Swap(GameObject newPrefab)
    {
        if (newPrefab == null) return;

        if (currentVisual != null)
            Destroy(currentVisual);

        currentVisual = Instantiate(newPrefab);
        currentVisual.transform.SetParent(visualsContainer, worldPositionStays: false);
        currentVisual.transform.localPosition = Vector3.zero;
        currentVisual.transform.localRotation = Quaternion.identity;
        currentVisual.transform.localScale = Vector3.one;

        SetLayerRecursively(currentVisual, playerLayer);

        foreach (var rb in currentVisual.GetComponentsInChildren<Rigidbody>(true))
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        foreach (var col in currentVisual.GetComponentsInChildren<Collider>(true))
        {
            col.isTrigger = true;
        }

        foreach (var anim in currentVisual.GetComponentsInChildren<Animator>(true))
        {
            anim.applyRootMotion = false;
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

        var follow = currentVisual.GetComponent<FollowParentRoot>();
        if (follow == null)
            follow = currentVisual.AddComponent<FollowParentRoot>();
        follow.SetTarget(visualsContainer);

        OnVisualChanged?.Invoke(currentVisual);

        if (playerInput != null)
            Debug.Log($"[OverlordSwap] Player {playerInput.playerIndex} swapped to {newPrefab.name}");
        else
            Debug.Log($"[OverlordSwap] Swapped to {newPrefab.name} (playerInput null)");
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform t in obj.GetComponentsInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }
}
