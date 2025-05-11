using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdManager : MonoBehaviour
{
    public static BirdManager Instance { get; private set; }

    private HashSet<GameObject> allBirds = new HashSet<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void RegisterBird(GameObject bird)
    {
        allBirds.Add(bird);
    }

    public void UnregisterBird(GameObject bird)
    {
        allBirds.Remove(bird);
    }

    public int GetBirdCount()
    {
        return allBirds.Count;
    }

    public HashSet<GameObject> GetAllBirds()
    {
        return allBirds;
    }
}

