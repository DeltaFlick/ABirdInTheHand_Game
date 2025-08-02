using System.Collections;
using UnityEngine;
using FMODUnity;

public class HumanAudioManager : MonoBehaviour
{
    [SerializeField] EventReference GrassFootstep;
    [SerializeField] float rate = 0.5f;

    private GameObject human;
    private PlayerControls controller;
    private float time;

    void Start()
    {
        controller = GetComponentInParent<PlayerControls>();
        human = controller != null ? controller.gameObject : null;

        if (controller == null)
            Debug.LogError("AudioManager: Couldn't find PlayerControls in parent objects of " + gameObject.name);

        if (GrassFootstep.IsNull)
            Debug.LogWarning("AudioManager: GrassFootstep EventReference is not set.");
    }


    public void PlayGrassFootstep()
    {
        RuntimeManager.PlayOneShotAttached(GrassFootstep, human);
    }

    void Update()
    {
        if (controller == null) return;

        time += Time.deltaTime;
        if (controller.isWalking && time >= rate)
        {
            PlayGrassFootstep();
            time = 0;
        }
    }
}
