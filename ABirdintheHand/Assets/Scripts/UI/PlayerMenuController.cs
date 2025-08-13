using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMenuController : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private GameObject preRoundMenuPanel;
    [SerializeField] private GameObject inRoundMenuPanel;

    [Header("Other UI")]
    [SerializeField] private GameObject crosshairCanvas;

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
    }

    private void OnEnable()
    {
        menuAction.performed += ToggleMenu;
        menuAction.Enable();
    }

    private void OnDisable()
    {
        menuAction.performed -= ToggleMenu;
    }

    private void ToggleMenu(InputAction.CallbackContext ctx)
    {
        menuOpen = !menuOpen;

        if (roundHasStarted)
        {
            if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(menuOpen);
            if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(false);
        }
        else
        {
            if (preRoundMenuPanel != null) preRoundMenuPanel.SetActive(menuOpen);
            if (inRoundMenuPanel != null) inRoundMenuPanel.SetActive(false);
        }

        if (crosshairCanvas != null) crosshairCanvas.SetActive(!menuOpen);

        if (playerControls != null) playerControls.SetControlsEnabled(!menuOpen);

        Cursor.lockState = menuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = menuOpen;
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
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnTitleScreenPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}
