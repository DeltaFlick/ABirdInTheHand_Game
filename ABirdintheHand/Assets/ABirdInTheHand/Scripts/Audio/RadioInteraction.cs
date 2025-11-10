using UnityEngine;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
using TMPro;

/// <summary>
/// Manage radio music & interaction sequence
/// </summary>
public class RadioInteraction : MonoBehaviour, IInteractable
{
    [Header("FMOD Songs")]
    [SerializeField] private EventReference happyDays;
    [SerializeField] private List<EventReference> otherSongs;
    [SerializeField] private GameObject radio;

    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    private EventInstance musicInstance;
    private List<EventReference> shufflePool;
    private EventReference currentSong;
    private bool isInitialized;

    private void Awake()
    {
        if (radio == null)
        {
            radio = gameObject;
        }
    }

    private void Start()
    {
        if (!happyDays.IsNull)
        {
            PlaySong(happyDays);
        }

        shufflePool = new List<EventReference>(otherSongs);

        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }

        isInitialized = true;
    }

    public void Interact(InteractionController interactor)
    {
        PlayNextShuffledSong();
    }

    public void ShowPrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    private void PlayNextShuffledSong()
    {
        if (shufflePool.Count == 0)
        {
            shufflePool = new List<EventReference>(otherSongs);
        }

        if (shufflePool.Count == 0)
        {
            Debug.LogWarning("[RadioInteraction] No songs available to play", this);
            return;
        }

        int index = Random.Range(0, shufflePool.Count);
        EventReference nextSong = shufflePool[index];
        shufflePool.RemoveAt(index);

        PlaySong(nextSong);
    }

    private void PlaySong(EventReference song)
    {
        if (song.IsNull)
        {
            Debug.LogWarning("[RadioInteraction] Attempted to play null song reference", this);
            return;
        }

        StopCurrentMusic();

        currentSong = song;

        try
        {
            musicInstance = RuntimeManager.CreateInstance(song);

            if (radio != null)
            {
                RuntimeManager.AttachInstanceToGameObject(musicInstance, radio.transform);
            }

            musicInstance.start();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RadioInteraction] Failed to play song: {e.Message}", this);
        }
    }

    private void StopCurrentMusic()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }

    private void OnDisable()
    {
        StopCurrentMusic();
    }

    private void OnDestroy()
    {
        StopCurrentMusic();
    }
}