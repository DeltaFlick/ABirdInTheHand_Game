using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepAlive : MonoBehaviour
{
    private GameObject sceneManager;

    private void Awake()
    {
        if (sceneManager == null)
        {
            sceneManager = gameObject;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);
    }
}