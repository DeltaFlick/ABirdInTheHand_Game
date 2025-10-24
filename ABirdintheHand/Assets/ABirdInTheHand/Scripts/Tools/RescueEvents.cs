using System;

/// <summary>
/// Rescue events with per-player targeting
/// </summary>

public static class RescueEvents
{
    public static Action<PlayerMenuController, float> OnRescueStarted;
    public static Action<PlayerMenuController, float> OnRescueUpdated;
    public static Action<PlayerMenuController> OnRescueEnded;
}
