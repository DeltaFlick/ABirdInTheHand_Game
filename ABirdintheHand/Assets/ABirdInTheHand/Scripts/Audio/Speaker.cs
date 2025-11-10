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
    private bool isRegistered;

    private void Awake()
    {
        if (speakerObject == null)
        {
            speakerObject = gameObject;
        }

        if (speakerController == null && autoRegisterToDesk)
        {
            speakerController = GetComponentInParent<SpeakerController>();
        }

        if (autoRegisterToDesk && speakerController != null)
        {
            speakerController.RegisterSpeaker(this);
            isRegistered = true;
        }
    }

    public void PlaySong(EventReference song)
    {
        if (song.IsNull)
        {
            Debug.LogWarning($"[Speaker] Attempted to play null song on {gameObject.name}", this);
            return;
        }

        StopMusic();

        currentSong = song;

        try
        {
            musicInstance = RuntimeManager.CreateInstance(song);

            if (speakerObject != null)
            {
                RuntimeManager.AttachInstanceToGameObject(musicInstance, speakerObject.transform);
            }

            musicInstance.start();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Speaker] Failed to play song on {gameObject.name}: {e.Message}", this);
        }
    }

    public void StopMusic()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }

    private void OnDisable()
    {
        StopMusic();
    }

    private void OnDestroy()
    {
        StopMusic();

        if (isRegistered && speakerController != null)
        {
            speakerController.UnregisterSpeaker(this);
            isRegistered = false;
        }
    }
}