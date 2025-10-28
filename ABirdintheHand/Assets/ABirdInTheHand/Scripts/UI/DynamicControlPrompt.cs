using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Updates UI prompts based on the player's control scheme
/// </summary>

public class DynamicControlPrompt : MonoBehaviour
{
    [Header("Control Images")]
    [SerializeField] private GameObject keyboardPrompt;
    [SerializeField] private GameObject gamepadPrompt;

    [Header("Alternative: Text-based")]
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private string keyboardText = "Press 'Z' to toggle menu";
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
        if (currentPlayerDetector == detector) return;

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

        foreach (var player in allPlayers)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
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
            if (keyboardPrompt != null)
                keyboardPrompt.SetActive(scheme == PlayerControlSchemeDetector.ControlScheme.KeyboardMouse);

            if (gamepadPrompt != null)
                gamepadPrompt.SetActive(scheme == PlayerControlSchemeDetector.ControlScheme.Gamepad);
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
            if (promptText != null) promptText.text = "";
        }
    }

    private void OnDestroy()
    {
        if (currentPlayerDetector != null)
        {
            currentPlayerDetector.OnControlSchemeChanged -= UpdatePrompt;
        }
    }
}