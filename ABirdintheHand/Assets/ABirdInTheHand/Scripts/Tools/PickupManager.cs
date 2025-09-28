using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupManager : MonoBehaviour
{
    public static event Action ForceDropAll;

    public static void RequestDropAll()
    {
        ForceDropAll?.Invoke();
    }
}
