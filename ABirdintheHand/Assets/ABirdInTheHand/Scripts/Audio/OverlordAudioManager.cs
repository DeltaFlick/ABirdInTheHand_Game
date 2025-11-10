using System;
using UnityEngine;
using FMODUnity;

/// <summary>
/// Modular audio manager for overlord character swapping
/// </summary>
[Serializable]
public class AudioProfile
{
    public string profileName;
    public EventReference footstep;
    public float footstepRate = 0.5f;
}

[RequireComponent(typeof(OverlordSwapHandler))]
[RequireComponent(typeof(PlayerControls))]
public class OverlordAudioManager : MonoBehaviour
{
    [Header("Audio Profiles")]
    [SerializeField] private AudioProfile[] audioProfiles;

    private OverlordSwapHandler swapHandler;
    private PlayerControls playerControls;
    private AudioProfile currentProfile;
    private GameObject currentVisual;
    private float footstepTimer;
    private bool wasWalking;

    private void Awake()
    {
        swapHandler = GetComponent<OverlordSwapHandler>();
        playerControls = GetComponent<PlayerControls>();

        if (swapHandler == null)
        {
            Debug.LogError("[OverlordAudioManager] OverlordSwapHandler component missing!", this);
            enabled = false;
            return;
        }

        if (playerControls == null)
        {
            Debug.LogError("[OverlordAudioManager] PlayerControls component missing!", this);
            enabled = false;
            return;
        }

        swapHandler.OnVisualChanged += OnVisualChanged;
        footstepTimer = 0f;
    }

    private void OnDestroy()
    {
        if (swapHandler != null)
        {
            swapHandler.OnVisualChanged -= OnVisualChanged;
        }
    }

    private void OnVisualChanged(GameObject newVisual)
    {
        currentVisual = newVisual;

        if (currentVisual == null || audioProfiles == null || audioProfiles.Length == 0)
        {
            currentProfile = null;
            return;
        }

        if (currentVisual.TryGetComponent<HumanIdentifier>(out _))
        {
            currentProfile = Array.Find(audioProfiles, p => p.profileName.Contains("Human"));
        }
        else if (currentVisual.TryGetComponent<BirdIdentifier>(out _))
        {
            currentProfile = Array.Find(audioProfiles, p => p.profileName.Contains("Bird"));
        }

        if (currentProfile == null && audioProfiles.Length > 0)
        {
            currentProfile = audioProfiles[0];
        }

        footstepTimer = 0f;
        wasWalking = false;
    }

    private void Update()
    {
        if (currentProfile == null || currentVisual == null)
            return;

        bool isWalking = playerControls.isWalking;

        if (isWalking)
        {
            footstepTimer += Time.deltaTime;

            if (footstepTimer >= currentProfile.footstepRate)
            {
                PlayFootstep();
                footstepTimer = 0f;
            }
        }
        else
        {
            if (wasWalking)
            {
                footstepTimer = 0f;
            }
        }

        wasWalking = isWalking;
    }

    private void PlayFootstep()
    {
        if (currentProfile == null || currentVisual == null || currentProfile.footstep.IsNull)
            return;

        RuntimeManager.PlayOneShotAttached(currentProfile.footstep, currentVisual);
    }
}