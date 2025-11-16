using UnityEngine;
using TMPro;

/// <summary>
/// Break objects on interact and award score
/// </summary>
public class BreakableInteraction : MonoBehaviour, IInteractable
{
    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Breakable Settings")]
    [SerializeField] private GameObject brokenVersion;
    [SerializeField] private float scoreAddAmount = 10f;

    private bool hasBeenBroken = false;

    private void Start()
    {
        if (promptText != null)
        {
            promptText.text = "Press [E] to Break";
        }
    }

    public void Interact(InteractionController interactor)
    {
        if (hasBeenBroken)
            return;

        hasBeenBroken = true;

        Debug.Log($"[BreakableInteraction] {gameObject.name} broken by {interactor?.name}", this);

        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.AddScore(scoreAddAmount);
        }
        else
        {
            Debug.LogWarning("[BreakableInteraction] ScoreSystem.Instance is null; score not added.", this);
        }

        if (brokenVersion != null)
        {
            Instantiate(brokenVersion, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    public void ShowPrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(true);
        }
    }

    public void HidePrompt()
    {
        if (promptUI != null)
        {
            promptUI.SetActive(false);
        }
    }
}