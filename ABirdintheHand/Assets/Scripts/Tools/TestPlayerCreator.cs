using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestPlayerCreator : MonoBehaviour
{
    [Header("Player Setup")]
    public GameObject playerPrefab;
    public PlayerManager playerManager;
    
    private PlayerInputManager playerInputManager;
    
    void Start()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();
        if (playerInputManager == null)
        {
            Debug.LogError("PlayerInputManager not found in scene!");
            return;
        }
        
        if (playerManager == null)
        {
            playerManager = FindObjectOfType<PlayerManager>();
        }
        
        if (playerPrefab != null)
        {
            playerInputManager.playerPrefab = playerPrefab;
        }
    }
    
    public void AddTestPlayer()
    {
        if (playerManager == null || playerPrefab == null)
        {
            Debug.LogError("Missing required references!");
            return;
        }
        
        GameObject playerObj = Instantiate(playerPrefab);
        
        
     
    }
    
    public void RemoveAllPlayers()
    {
        if (playerManager == null)
            playerManager = FindObjectOfType<PlayerManager>();
            
        var allPlayers = FindObjectsOfType<PlayerInput>().ToList();
        
        foreach (var player in allPlayers)
        {
            DestroyImmediate(player.gameObject);
        }
        
        Debug.Log("Removed all test players");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestPlayerCreator))]
public class TestPlayerCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Testing Controls", EditorStyles.boldLabel);
        
        TestPlayerCreator creator = (TestPlayerCreator)target;
        
        if (GUILayout.Button("Add Test Player"))
        {
            creator.AddTestPlayer();
        }
        
        if (GUILayout.Button("Remove All Players"))
        {
            creator.RemoveAllPlayers();
        }
    }
}
#endif