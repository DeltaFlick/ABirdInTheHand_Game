using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleInteractable : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Interacted with: " + gameObject.name);
    }
}
