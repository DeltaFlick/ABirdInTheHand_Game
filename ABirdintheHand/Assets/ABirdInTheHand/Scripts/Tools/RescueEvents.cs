using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <Summary>
/// A collection for rescue events
/// </Summary>


public static class RescueEvents
{
    public static Action<float> OnRescueStarted;
    public static Action<float> OnRescueUpdated;
    public static Action OnRescueEnded;
}
