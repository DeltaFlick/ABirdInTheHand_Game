using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <Summary>
/// Pickup system force drop
/// </Summary>

public class ForceDrop : MonoBehaviour
{
    public static event Action ForceDropAll;

    public static void RequestDropAll()
    {
        ForceDropAll?.Invoke();
    }
}
