using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdIdentifier : MonoBehaviour
{

    public bool IsBeingHeld { get; set; }
    public bool IsCaged { get; set; }
    public PlayerMenuController MenuController { get; private set; }

    void Awake()
    {
        MenuController = GetComponent<PlayerMenuController>();
        if (MenuController == null)
        {
            MenuController = GetComponentInChildren<PlayerMenuController>();
        }
    }

    void Start()
    {
        BirdManager.Instance?.RegisterBird(this.gameObject);
    }

    void OnDestroy()
    {
        BirdManager.Instance?.UnregisterBird(this.gameObject);
    }
}
