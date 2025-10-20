using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class PlayerMenuController : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private GameObject preRoundMenuPanel;
    [SerializeField] private GameObject inRoundMenuPanel;

    [Header("Other UI")]
    [SerializeField] private GameObject crosshairCanvas;
    [SerializeField] private Slider rescueTimerSlider;

    [Header("First Selected")]
    [SerializeField] private Button preRoundSelectedElement;
    [SerializeField] private Button inRoundSelectedElement;

    private PlayerInput playerInput;
    private InputAction menuAction;
    private InputSystemUIInputModule uiModule;

    private bool menuOpen = false;
    private bool roundHasStarted = false;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        uiModule = GetComponentInChildren<InputSystemUIInputModule>(true);

        playerInput.actions.FindActionMap("UI")?.Enable();
        menuAction = playerInput.actions.FindAction("UI/Menu");

        if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(false);
        if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(false);
        if (rescueTimerSlider != null) rescueTimerSlider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (menuAction != null) menuAction.performed += ToggleMenu;

        RescueEvents.OnRescueStarted += ShowRescueTimer;
        RescueEvents.OnRescueUpdated += UpdateRescueTimer;
        RescueEvents.OnRescueEnded += HideRescueTimer;
    }

    private void OnDisable()
    {
        if (menuAction != null) menuAction.performed -= ToggleMenu;

        RescueEvents.OnRescueStarted -= ShowRescueTimer;
        RescueEvents.OnRescueUpdated -= UpdateRescueTimer;
        RescueEvents.OnRescueEnded -= HideRescueTimer;
    }

    private void ToggleMenu(InputAction.CallbackContext ctx)
    {
        menuOpen = !menuOpen;

        if (roundHasStarted)
        {
            if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(menuOpen);
            SetSelected(inRoundSelectedElement);
            if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(false);
        }
        else
        {
            if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(menuOpen);
            SetSelected(preRoundSelectedElement);
            if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(false);
        }

        if (crosshairCanvas != null) crosshairCanvas.SetActive(!menuOpen);

        var controls = GetComponent<PlayerControls>();
        if (controls != null) controls.SetControlsEnabled(!menuOpen);
    }

    private void SetSelected(Button button)
    {
        if (button != null) button.Select();
    }

    public void SetRoundStarted(bool started)
    {
        roundHasStarted = started;
        if (menuOpen) ToggleMenu(new InputAction.CallbackContext());
    }

    #region Rescue Timer
    public void ShowRescueTimer(float maxTime)
    {
        if (rescueTimerSlider == null) return;
        rescueTimerSlider.maxValue = maxTime;
        rescueTimerSlider.value = maxTime;
        rescueTimerSlider.gameObject.SetActive(true);
    }

    public void UpdateRescueTimer(float timeLeft)
    {
        if (rescueTimerSlider == null) return;
        rescueTimerSlider.value = timeLeft;
    }

    public void HideRescueTimer()
    {
        if (rescueTimerSlider == null) return;
        rescueTimerSlider.gameObject.SetActive(false);
    }
    #endregion

    #region Character Swap UI Buttons
    public void OnCharacterButtonPressed(int index)
    {
        GetComponent<PlayerSwapHandler>()?.SwapToCharacter(index);
    }

    public void OnCharacterButtonPressed(string characterName)
    {
        GetComponent<PlayerSwapHandler>()?.SwapToCharacter(characterName);
    }
    #endregion
}
