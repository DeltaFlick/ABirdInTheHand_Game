using UnityEngine;
using TMPro;

/// <summary>
/// Updates UI prompts based on the player's control scheme (keyboard vs gamepad)
/// </summary>
public class DynamicControlPrompt : MonoBehaviour
{
    [Header("Control Images")]
    [SerializeField] private GameObject keyboardPrompt;
    [SerializeField] private GameObject gamepadPrompt;

    [Header("Alternative: Text-based")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string keyboardText = "Press 'Esc' to toggle menu";
    [SerializeField] private string gamepadText = "Press 'SELECT' to toggle menu";

    [Header("Settings")]
    [SerializeField] private bool useImagePrompts = true;
    [SerializeField] private bool autoFindPlayer = true;
    [SerializeField] private float updateInterval = 1f;

    private PlayerControlSchemeDetector currentPlayerDetector;
    private float nextUpdateTime;

    private void Start()
    {
        if (useImagePrompts)
        {
            if (keyboardPrompt != null) keyboardPrompt.SetActive(false);
            if (gamepadPrompt != null) gamepadPrompt.SetActive(false);
        }
        else
        {
            if (promptText != null) promptText.text = string.Empty;
        }
    }

    private void Update()
    {
        if (autoFindPlayer && Time.time >= nextUpdateTime)
        {
            nextUpdateTime = Time.time + updateInterval;
            FindClosestPlayer();
        }
    }

    public void SetPlayer(PlayerControlSchemeDetector detector)
    {
        if (currentPlayerDetector == detector)
            return;

        if (currentPlayerDetector != null)
        {
            currentPlayerDetector.OnControlSchemeChanged -= UpdatePrompt;
        }

        currentPlayerDetector = detector;

        if (currentPlayerDetector != null)
        {
            currentPlayerDetector.OnControlSchemeChanged += UpdatePrompt;
            UpdatePrompt(currentPlayerDetector.CurrentControlScheme);
        }
        else
        {
            HidePrompts();
        }
    }

    private void FindClosestPlayer()
    {
        PlayerControlSchemeDetector[] allPlayers = FindObjectsOfType<PlayerControlSchemeDetector>();

        if (allPlayers.Length == 0)
        {
            SetPlayer(null);
            return;
        }

        float closestDistance = Mathf.Infinity;
        PlayerControlSchemeDetector closest = null;
        Vector3 myPosition = transform.position;

        foreach (var player in allPlayers)
        {
            if (player == null) continue;

            float distance = Vector3.Distance(myPosition, player.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = player;
            }
        }

        SetPlayer(closest);
    }

    private void UpdatePrompt(PlayerControlSchemeDetector.ControlScheme scheme)
    {
        if (useImagePrompts)
        {
            bool isKeyboard = scheme == PlayerControlSchemeDetector.ControlScheme.KeyboardMouse;

            if (keyboardPrompt != null)
            {
                keyboardPrompt.SetActive(isKeyboard);
            }

            if (gamepadPrompt != null)
            {
                gamepadPrompt.SetActive(!isKeyboard);
            }
        }
        else
        {
            if (promptText != null)
            {
                promptText.text = scheme == PlayerControlSchemeDetector.ControlScheme.KeyboardMouse
                    ? keyboardText
                    : gamepadText;
            }
        }
    }

    private void HidePrompts()
    {
        if (useImagePrompts)
        {
            if (keyboardPrompt != null) keyboardPrompt.SetActive(false);
            if (gamepadPrompt != null) gamepadPrompt.SetActive(false);
        }
        else
        {
            if (promptText != null) promptText.text = string.Empty;
        }
    }

    private void OnDestroy()
    {
        if (currentPlayerDetector != null)
        {
            currentPlayerDetector.OnControlSchemeChanged -= UpdatePrompt;
            currentPlayerDetector = null;
        }
    }
}