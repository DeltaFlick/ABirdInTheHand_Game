using System;
using UnityEngine;
using FMODUnity;

[Serializable]
public class AudioProfile
{
    public string profileName;       // Name of the visual prefab
    public EventReference footstep;  // Footstep sound
    public float footstepRate = 0.5f; // Seconds between footsteps
    // Add more sounds here if needed, e.g., jump, landing, attack
}

/// <summary>
/// Modular audio manager compatible with Overlord characters
/// </summary>
[RequireComponent(typeof(OverlordSwapHandler))]
[RequireComponent(typeof(PlayerControls))]
public class OverlordAudioManager : MonoBehaviour
{
    [Header("Audio Profiles for Each Visual")]
    [SerializeField] private AudioProfile[] audioProfiles;

    private OverlordSwapHandler swapHandler;
    private PlayerControls playerControls;
    private AudioProfile currentProfile;
    private GameObject currentVisual;
    private float footstepTimer;

    private void Awake()
    {
        swapHandler = GetComponent<OverlordSwapHandler>();
        playerControls = GetComponent<PlayerControls>();

        if (swapHandler != null)
            swapHandler.OnVisualChanged += OnVisualChanged;

        footstepTimer = 0f;
    }

    private void OnDestroy()
    {
        if (swapHandler != null)
            swapHandler.OnVisualChanged -= OnVisualChanged;
    }

    private void OnVisualChanged(GameObject newVisual)
    {
        currentVisual = newVisual;

        if (currentVisual == null || audioProfiles == null || audioProfiles.Length == 0)
        {
            currentProfile = null;
            return;
        }

        // Use the identifiers to determine which profile to use
        if (currentVisual.GetComponent<HumanIdentifier>() != null)
        {
            // Use the first AudioProfile with "Human" in the profileName (or assign manually)
            currentProfile = Array.Find(audioProfiles, p => p.profileName.Contains("Human"));
        }
        else if (currentVisual.GetComponent<BirdIdentifier>() != null)
        {
            // Use the first AudioProfile with "Bird" in the profileName
            currentProfile = Array.Find(audioProfiles, p => p.profileName.Contains("Bird"));
        }

        // Fallback if nothing matched
        if (currentProfile == null)
            currentProfile = audioProfiles[0];

        footstepTimer = 0f;
    }

    private void Update()
    {
        if (playerControls == null || currentProfile == null || currentVisual == null)
            return;

        footstepTimer += Time.deltaTime;

        // Play footstep if character is walking and timer reached
        if (playerControls.isWalking && footstepTimer >= currentProfile.footstepRate)
        {
            PlayFootstep();
            footstepTimer = 0f;
        }
        else if (!playerControls.isWalking)
        {
            // Reset timer to prevent instant footsteps when resuming movement
            footstepTimer = currentProfile.footstepRate;
        }
    }

    public void PlayFootstep()
    {
        if (currentProfile == null || currentVisual == null || currentProfile.footstep.IsNull)
            return;

        RuntimeManager.PlayOneShotAttached(currentProfile.footstep, currentVisual);
        Debug.Log($"[OverlordAudioManager] Played footstep for {currentVisual.name}");
    }
}
