using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMenuController : MonoBehaviour
{
    [Header("Menu UI")]
    [SerializeField] private Canvas menuCanvas;
    [SerializeField] private GameObject menuPanel;

    [Header("Other UI")]
    [SerializeField] private GameObject crosshairCanvas;

    private bool menuOpen = false;
    private PlayerControls playerControls;
    private InputAction menuAction;

    private void Awake()
    {
        playerControls = GetComponent<PlayerControls>();
        var inputAsset = GetComponent<PlayerInput>().actions;

        menuAction = inputAsset.FindActionMap("UI").FindAction("Menu");

        if (menuPanel != null)
            menuPanel.SetActive(false);
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

        if (menuPanel != null) menuPanel.SetActive(menuOpen);
        if (crosshairCanvas != null) crosshairCanvas.SetActive(!menuOpen);

        if (playerControls != null) playerControls.SetControlsEnabled(!menuOpen);

        Cursor.lockState = menuOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = menuOpen;
    }
}
