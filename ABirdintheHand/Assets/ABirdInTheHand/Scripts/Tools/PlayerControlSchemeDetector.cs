using UnityEngine;
using UnityEngine.InputSystem;
using System;

/// <summary>
/// Detects and tracks the control scheme for a player
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerControlSchemeDetector : MonoBehaviour
{
    public enum ControlScheme
    {
        KeyboardMouse,
        Gamepad,
        Unknown
    }

    public event Action<ControlScheme> OnControlSchemeChanged;

    private PlayerInput playerInput;
    private ControlScheme currentControlScheme = ControlScheme.Unknown;

    public ControlScheme CurrentControlScheme => currentControlScheme;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        if (playerInput == null)
        {
            Debug.LogError("[PlayerControlSchemeDetector] PlayerInput component missing!", this);
            enabled = false;
            return;
        }

        playerInput.onControlsChanged += OnControlsChanged;

        DetectControlScheme();
    }

    private void OnControlsChanged(PlayerInput input)
    {
        DetectControlScheme();
    }

    private void DetectControlScheme()
    {
        if (playerInput == null) return;

        ControlScheme newScheme = ControlScheme.Unknown;
        string schemeName = playerInput.currentControlScheme;

        if (string.IsNullOrEmpty(schemeName))
        {
            return;
        }

        if (schemeName.Contains("Keyboard") || schemeName.Contains("Mouse"))
        {
            newScheme = ControlScheme.KeyboardMouse;
        }
        else if (schemeName.Contains("Gamepad") || schemeName.Contains("Controller"))
        {
            newScheme = ControlScheme.Gamepad;
        }

        if (newScheme != currentControlScheme && newScheme != ControlScheme.Unknown)
        {
            currentControlScheme = newScheme;
            OnControlSchemeChanged?.Invoke(currentControlScheme);

            Debug.Log($"[PlayerControlSchemeDetector] Player {playerInput.playerIndex} scheme: {currentControlScheme}", this);
        }
    }

    public bool IsUsingKeyboardMouse()
    {
        return currentControlScheme == ControlScheme.KeyboardMouse;
    }

    public bool IsUsingGamepad()
    {
        return currentControlScheme == ControlScheme.Gamepad;
    }

    private void OnDestroy()
    {
        if (playerInput != null)
        {
            playerInput.onControlsChanged -= OnControlsChanged;
        }
    }
}