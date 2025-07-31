using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] EventReference LobbyMusic;

    private void OnTriggerEnter(Collider other)
    {
        PlayLobbyMusic();
    }

    public void PlayLobbyMusic()
    {
        RuntimeManager.PlayOneShot(LobbyMusic);
    }
}