using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using FMOD.Studio;
using TMPro;

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

    private void Start()
    {
        PlaySong(happyDays);
        shufflePool = new List<EventReference>(otherSongs);

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    public void Interact()
    {
        PlayNextShuffledSong();
    }

    public void ShowPrompt()
    {
        if (promptUI != null) promptUI.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    private void PlayNextShuffledSong()
    {
        if (shufflePool.Count == 0)
            shufflePool = new List<EventReference>(otherSongs);

        if (shufflePool.Count == 0)
            return;

        int index = Random.Range(0, shufflePool.Count);
        EventReference nextSong = shufflePool[index];
        shufflePool.RemoveAt(index);

        PlaySong(nextSong);
    }

    private void PlaySong(EventReference song)
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }

        currentSong = song;
        musicInstance = RuntimeManager.CreateInstance(song);
        RuntimeManager.AttachInstanceToGameObject(musicInstance, radio);
        musicInstance.start();
    }

    private void OnDestroy()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }
}
