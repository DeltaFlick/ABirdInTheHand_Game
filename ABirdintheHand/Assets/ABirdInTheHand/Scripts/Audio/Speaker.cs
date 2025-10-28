using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Individual speaker that plays music from a mixing desk
/// </summary>

public class Speaker : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject speakerObject;
    [SerializeField] private bool autoRegisterToDesk = true;
    [SerializeField] private SpeakerController speakerController;

    private EventInstance musicInstance;
    private EventReference currentSong;

    private void Awake()
    {
        if (speakerObject == null)
            speakerObject = gameObject;

        if (autoRegisterToDesk && speakerController != null)
        {
            speakerController.RegisterSpeaker(this);
        }
    }

    public void PlaySong(EventReference song)
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }

        currentSong = song;

        musicInstance = RuntimeManager.CreateInstance(song);
        RuntimeManager.AttachInstanceToGameObject(musicInstance, speakerObject.transform);
        musicInstance.start();
    }

    public void StopMusic()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }

    private void OnDestroy()
    {
        StopMusic();

        if (speakerController != null)
            speakerController.UnregisterSpeaker(this);
    }
}