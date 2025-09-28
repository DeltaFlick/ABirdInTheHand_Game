using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class MusicManager : MonoBehaviour
{
    [SerializeField] EventReference HappyDays;
    [SerializeField] GameObject radio;

    void Start()
    {
        PlayHappyDays();
    }

    public void PlayHappyDays()
    {
        RuntimeManager.PlayOneShotAttached(HappyDays, radio);
    }
}
