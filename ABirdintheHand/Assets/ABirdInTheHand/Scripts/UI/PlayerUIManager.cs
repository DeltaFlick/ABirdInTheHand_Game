using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Manages per-player UI input module and event system
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerUIManager : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private InputSystemUIInputModule uiModule;
    [SerializeField] private EventSystem eventSystem;

    private PlayerInput playerInput;

    public EventSystem EventSystem => eventSystem;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("[PlayerUIManager] PlayerInput component missing!", this);
            enabled = false;
            return;
        }

        if (uiModule == null)
        {
            uiModule = GetComponentInChildren<InputSystemUIInputModule>(true);
        }

        if (uiModule == null)
        {
            GameObject uiGO = new GameObject("UIInputModule");
            uiGO.transform.SetParent(transform);
            uiModule = uiGO.AddComponent<InputSystemUIInputModule>();
        }

        if (eventSystem == null)
        {
            eventSystem = GetComponentInChildren<EventSystem>(true);
        }

        if (eventSystem == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.transform.SetParent(transform);
            eventSystem = esGO.AddComponent<EventSystem>();
        }

        SetupUIModule();
    }

    private void SetupUIModule()
    {
        if (uiModule == null || playerInput == null)
        {
            Debug.LogWarning("[PlayerUIManager] Cannot setup UI module - missing components", this);
            return;
        }

        uiModule.actionsAsset = playerInput.actions;
        uiModule.deselectOnBackgroundClick = true;
        uiModule.enabled = false;

        if (eventSystem != null)
        {
            eventSystem.sendNavigationEvents = true;
            eventSystem.pixelDragThreshold = 5;
            eventSystem.enabled = true;
        }
    }

    public void SetUIEnabled(bool enabled)
    {
        if (uiModule != null)
        {
            uiModule.enabled = enabled;
        }

        if (eventSystem != null)
        {
            eventSystem.enabled = enabled;
        }
    }

    private void OnDestroy()
    {
        SetUIEnabled(false);
    }
}