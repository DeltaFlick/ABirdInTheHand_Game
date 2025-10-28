using UnityEngine;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
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
    private int currentSongIndex = -1;

    private void Awake()
    {
        shufflePool = new List<EventReference>(availableSongs);

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void Start()
    {
        if (defaultSong.IsNull == false)
        {
            currentSong = defaultSong;
            PlaySongOnAllSpeakers(defaultSong);
        }
    }

    public void Interact(InteractionController interactor)
    {
        PlayNextShuffledSong();
    }

    public void ShowPrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void PlayNextShuffledSong()
    {
        if (shufflePool.Count == 0)
            shufflePool = new List<EventReference>(availableSongs);

        if (shufflePool.Count == 0)
            return;

        int index = Random.Range(0, shufflePool.Count);
        EventReference nextSong = shufflePool[index];
        shufflePool.RemoveAt(index);

        currentSong = nextSong;
        PlaySongOnAllSpeakers(nextSong);
    }

    private void PlaySongOnAllSpeakers(EventReference song)
    {
        foreach (Speaker speaker in connectedSpeakers)
        {
            if (speaker != null)
                speaker.PlaySong(song);
        }
    }

    public void RegisterSpeaker(Speaker speaker)
    {
        if (!connectedSpeakers.Contains(speaker))
        {
            connectedSpeakers.Add(speaker);

            if (currentSong.IsNull == false)
                speaker.PlaySong(currentSong);
        }
    }

    public void UnregisterSpeaker(Speaker speaker)
    {
        connectedSpeakers.Remove(speaker);
    }

    private void OnDestroy()
    {
        foreach (Speaker speaker in connectedSpeakers)
        {
            if (speaker != null)
                speaker.StopMusic();
        }
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;

        if (connectedSpeakers.Count == 0)
        {
            connectedSpeakers.AddRange(GetComponentsInChildren<Speaker>());
        }
    }
}