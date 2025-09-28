using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private MultiplayerEventSystem eventSystem;
    [SerializeField] private GameObject preRoundSelectedElement;
    [SerializeField] private GameObject postRoundSelectedElement;

    private bool menuOpen = false;
    private bool roundHasStarted = false;
    private PlayerControls playerControls;
    private InputAction menuAction;

    private void Awake()
    {
        playerControls = GetComponent<PlayerControls>();
        var inputAsset = GetComponent<PlayerInput>().actions;

        menuAction = inputAsset.FindActionMap("UI").FindAction("Menu");

        if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(false);
        if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(false);

        if (rescueTimerSlider != null)
            rescueTimerSlider.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        menuAction.performed += ToggleMenu;
        menuAction.Enable();

        RescueEvents.OnRescueStarted += ShowRescueTimer;
        RescueEvents.OnRescueUpdated += UpdateRescueTimer;
        RescueEvents.OnRescueEnded += HideRescueTimer;
    }

    private void OnDisable()
    {
        menuAction.performed -= ToggleMenu;

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
            eventSystem.SetSelectedGameObject(postRoundSelectedElement);
            if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(false);
        }
        else
        {
            if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(menuOpen);
            eventSystem.SetSelectedGameObject(preRoundSelectedElement);
            if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(false);
        }

        if (crosshairCanvas != null) crosshairCanvas.SetActive(!menuOpen);

        if (playerControls != null) playerControls.SetControlsEnabled(!menuOpen);

    }

    public void SetRoundStarted(bool started)
    {
        roundHasStarted = started;
        if (menuOpen) ToggleMenu(new InputAction.CallbackContext());
    }

    public void OnResumePressed()
    {
        menuOpen = false;
        if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(false);
        if (crosshairCanvas != null) crosshairCanvas.SetActive(true);
        if (playerControls != null) playerControls.SetControlsEnabled(true);
    }

    public void OnTitleScreenPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

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
}
