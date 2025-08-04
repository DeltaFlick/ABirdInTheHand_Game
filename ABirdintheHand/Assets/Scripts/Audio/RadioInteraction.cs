using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class RadioInteraction : MonoBehaviour, IInteractable
{
    [SerializeField] private EventReference HappyDays;
    [SerializeField] private GameObject radio;

    private EventInstance musicInstance;
    private bool isPaused = false;

    private void Start()
    {
        musicInstance = RuntimeManager.CreateInstance(HappyDays);
        RuntimeManager.AttachInstanceToGameObject(musicInstance, radio.transform, radio.GetComponent<Rigidbody>());
        musicInstance.start();
    }

    public void Interact()
    {
        if (!musicInstance.isValid())
            return;

        isPaused = !isPaused;
        musicInstance.setPaused(isPaused);
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
