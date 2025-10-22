using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

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

        if (uiModule == null)
            uiModule = GetComponentInChildren<InputSystemUIInputModule>(true);

        if (eventSystem == null)
            eventSystem = GetComponentInChildren<EventSystem>(true);

        if (uiModule == null)
        {
            GameObject uiGO = new GameObject("UIInputModule");
            uiGO.transform.SetParent(transform);
            uiModule = uiGO.AddComponent<InputSystemUIInputModule>();
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
        if (uiModule == null || playerInput == null) return;

        uiModule.actionsAsset = playerInput.actions;

        uiModule.deselectOnBackgroundClick = true;

        eventSystem.sendNavigationEvents = true;
        eventSystem.pixelDragThreshold = 5;

        eventSystem.enabled = true;

        uiModule.enabled = false;
    }

    public void SetUIEnabled(bool enabled)
    {
        if (uiModule != null)
            uiModule.enabled = enabled;

        if (eventSystem != null)
            eventSystem.enabled = enabled;
    }
}
