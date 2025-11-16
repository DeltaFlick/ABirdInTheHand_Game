using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton manager to track all birds in the scene
/// </summary>
public class BirdManager : MonoBehaviour
{
    public static BirdManager Instance { get; private set; }

    private HashSet<GameObject> allBirds = new HashSet<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[BirdManager] Duplicate BirdManager detected, destroying", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterBird(GameObject bird)
    {
        if (bird == null)
        {
            Debug.LogWarning("[BirdManager] Attempted to register null bird!", this);
            return;
        }

        if (allBirds.Add(bird))
        {
            Debug.Log($"[BirdManager] Registered bird: {bird.name}. Total: {allBirds.Count}", this);
        }
    }

    public void UnregisterBird(GameObject bird)
    {
        if (bird == null)
            return;

        if (allBirds.Remove(bird))
        {
            Debug.Log($"[BirdManager] Unregistered bird: {bird.name}. Total: {allBirds.Count}", this);
        }
    }

    public int GetBirdCount()
    {
        allBirds.RemoveWhere(b => b == null);
        return allBirds.Count;
    }

    public HashSet<GameObject> GetAllBirds()
    {
        allBirds.RemoveWhere(b => b == null);
        return new HashSet<GameObject>(allBirds);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        allBirds.Clear();
    }
}