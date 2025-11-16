using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Manages player menu UI, rescue timer, and character swap buttons
/// </summary>
[RequireComponent(typeof(PlayerUIManager))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMenuController : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private GameObject preRoundMenuPanel;
    [SerializeField] private GameObject inRoundMenuPanel;

    [Header("Other UI")]
    [SerializeField] private GameObject crosshairCanvas;
    [SerializeField] private Slider rescueTimerSlider;

    [Header("First Selected Buttons")]
    [SerializeField] private Button preRoundSelectedElement;
    [SerializeField] private Button inRoundSelectedElement;

    private PlayerInput playerInput;
    private InputAction menuAction;
    private PlayerUIManager uiManager;
    private PlayerControls playerControls;
    private OverlordSwapHandler swapHandler;

    private bool menuOpen = false;
    private bool roundHasStarted = false;

    private System.Action<PlayerMenuController, float> onRescueStartedHandler;
    private System.Action<PlayerMenuController, float> onRescueUpdatedHandler;
    private System.Action<PlayerMenuController> onRescueEndedHandler;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        swapHandler = GetComponent<OverlordSwapHandler>();
        uiManager = GetComponent<PlayerUIManager>();
        playerControls = GetComponent<PlayerControls>();

        if (playerInput == null || playerInput.actions == null)
        {
            Debug.LogError("[PlayerMenuController] PlayerInput or actions missing!", this);
            enabled = false;
            return;
        }

        InputActionMap uiMap = playerInput.actions.FindActionMap("UI");
        if (uiMap != null)
        {
            uiMap.Enable();
        }
        else
        {
            Debug.LogWarning("[PlayerMenuController] UI action map not found!", this);
        }

        menuAction = playerInput.actions.FindAction("UI/Menu");

        if (menuAction == null)
        {
            Debug.LogError("[PlayerMenuController] Menu action not found!", this);
            enabled = false;
            return;
        }

        if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(false);
        if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(false);
        if (rescueTimerSlider != null) rescueTimerSlider.gameObject.SetActive(false);

        onRescueStartedHandler = (menu, time) => { if (menu == this) ShowRescueTimer(time); };
        onRescueUpdatedHandler = (menu, time) => { if (menu == this) UpdateRescueTimer(time); };
        onRescueEndedHandler = menu => { if (menu == this) HideRescueTimer(); };
    }

    private void OnEnable()
    {
        if (menuAction != null)
        {
            menuAction.performed += ToggleMenu;
        }

        RescueEvents.OnRescueStarted += onRescueStartedHandler;
        RescueEvents.OnRescueUpdated += onRescueUpdatedHandler;
        RescueEvents.OnRescueEnded += onRescueEndedHandler;
    }

    private void OnDisable()
    {
        if (menuAction != null)
        {
            menuAction.performed -= ToggleMenu;
        }

        RescueEvents.OnRescueStarted -= onRescueStartedHandler;
        RescueEvents.OnRescueUpdated -= onRescueUpdatedHandler;
        RescueEvents.OnRescueEnded -= onRescueEndedHandler;

        if (menuOpen)
        {
            CloseMenu();
        }
    }

    private void ToggleMenu(InputAction.CallbackContext ctx)
    {
        menuOpen = !menuOpen;

        if (menuOpen)
        {
            OpenMenu();
        }
        else
        {
            CloseMenu();
        }
    }

    private void OpenMenu()
    {
        if (roundHasStarted)
        {
            if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(true);
            if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(false);
            SetSelected(inRoundSelectedElement);
        }
        else
        {
            if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(true);
            if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(false);
            SetSelected(preRoundSelectedElement);
        }

        if (crosshairCanvas != null)
        {
            crosshairCanvas.SetActive(false);
        }

        if (playerControls != null)
        {
            playerControls.SetControlsEnabled(false);
        }

        if (uiManager != null)
        {
            uiManager.SetUIEnabled(true);
        }
    }

    private void CloseMenu()
    {
        if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(false);
        if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(false);

        if (crosshairCanvas != null)
        {
            crosshairCanvas.SetActive(true);
        }

        if (playerControls != null)
        {
            playerControls.SetControlsEnabled(true);
        }

        if (uiManager != null)
        {
            uiManager.SetUIEnabled(false);
        }
    }

    private void SetSelected(Button button)
    {
        if (button == null || uiManager == null || !menuOpen)
            return;

        EventSystem es = uiManager.EventSystem;
        if (es != null)
        {
            if (es.currentSelectedGameObject != button.gameObject)
            {
                es.SetSelectedGameObject(null);
                es.SetSelectedGameObject(button.gameObject);
            }
        }
    }

    public void SetRoundStarted(bool started)
    {
        roundHasStarted = started;

        if (menuOpen)
        {
            menuOpen = false;
            CloseMenu();
        }
    }

    #region Rescue Timer

    public void ShowRescueTimer(float maxTime)
    {
        if (rescueTimerSlider == null)
        {
            Debug.LogWarning($"[PlayerMenuController] RescueSlider not assigned on {name}", this);
            return;
        }

        rescueTimerSlider.maxValue = maxTime;
        rescueTimerSlider.value = maxTime;
        rescueTimerSlider.gameObject.SetActive(true);
    }

    public void UpdateRescueTimer(float timeLeft)
    {
        if (rescueTimerSlider != null && rescueTimerSlider.gameObject.activeInHierarchy)
        {
            rescueTimerSlider.value = timeLeft;
        }
    }

    public void HideRescueTimer()
    {
        if (rescueTimerSlider != null)
        {
            rescueTimerSlider.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Character Swap UI Buttons

    public void OnCharacterButtonPressed(int index)
    {
        if (swapHandler != null)
        {
            swapHandler.SwapToCharacter(index);
        }
        else
        {
            Debug.LogWarning("[PlayerMenuController] No OverlordSwapHandler found!", this);
        }
    }

    public void OnCharacterButtonPressed(string characterName)
    {
        if (swapHandler != null)
        {
            swapHandler.SwapToCharacter(characterName);
        }
        else
        {
            Debug.LogWarning("[PlayerMenuController] No OverlordSwapHandler found!", this);
        }
    }

    #endregion

    private void OnDestroy()
    {
        if (menuOpen)
        {
            CloseMenu();
        }
    }
}