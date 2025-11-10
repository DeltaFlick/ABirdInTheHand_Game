using UnityEngine;
using System.Collections.Generic;
using FMODUnity;
using TMPro;

/// <summary>
/// Mixing desk that controls connected speakers
/// </summary>
public class SpeakerController : MonoBehaviour, IInteractable
{
    [Header("FMOD Songs")]
    [SerializeField] private EventReference defaultSong;
    [SerializeField] private List<EventReference> availableSongs;

    [Header("Connected Speakers")]
    [SerializeField] private List<Speaker> connectedSpeakers = new List<Speaker>();

    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    private List<EventReference> shufflePool;
    private EventReference currentSong;
    private bool isInitialized;

    private void Awake()
    {
        shufflePool = new List<EventReference>(availableSongs);

        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }

    private void Start()
    {
        if (!defaultSong.IsNull)
        {
            currentSong = defaultSong;
            PlaySongOnAllSpeakers(defaultSong);
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
            shufflePool = new List<EventReference>(availableSongs);
        }

        if (shufflePool.Count == 0)
        {
            Debug.LogWarning("[SpeakerController] No songs available to play", this);
            return;
        }

        int index = Random.Range(0, shufflePool.Count);
        EventReference nextSong = shufflePool[index];
        shufflePool.RemoveAt(index);

        currentSong = nextSong;
        PlaySongOnAllSpeakers(nextSong);
    }

    private void PlaySongOnAllSpeakers(EventReference song)
    {
        if (song.IsNull)
        {
            Debug.LogWarning("[SpeakerController] Attempted to play null song", this);
            return;
        }

        connectedSpeakers.RemoveAll(s => s == null);

        foreach (Speaker speaker in connectedSpeakers)
        {
            speaker.PlaySong(song);
        }
    }

    public void RegisterSpeaker(Speaker speaker)
    {
        if (speaker == null)
        {
            Debug.LogWarning("[SpeakerController] Attempted to register null speaker", this);
            return;
        }

        if (!connectedSpeakers.Contains(speaker))
        {
            connectedSpeakers.Add(speaker);

            if (isInitialized && !currentSong.IsNull)
            {
                speaker.PlaySong(currentSong);
            }
        }
    }

    public void UnregisterSpeaker(Speaker speaker)
    {
        if (speaker != null)
        {
            connectedSpeakers.Remove(speaker);
        }
    }

    private void StopAllSpeakers()
    {
        connectedSpeakers.RemoveAll(s => s == null);

        foreach (Speaker speaker in connectedSpeakers)
        {
            speaker.StopMusic();
        }
    }

    private void OnDisable()
    {
        StopAllSpeakers();
    }

    private void OnDestroy()
    {
        StopAllSpeakers();
        connectedSpeakers.Clear();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        
        // Auto-populate speakers in editor
        if (connectedSpeakers.Count == 0)
        {
            Speaker[] speakers = GetComponentsInChildren<Speaker>();
            connectedSpeakers.AddRange(speakers);
        }
    }
#endif
}