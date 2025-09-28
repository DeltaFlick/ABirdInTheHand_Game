using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BreakableInteraction : MonoBehaviour, IInteractable
{
    [Header("UI Prompt")]
    [SerializeField] private GameObject promptUI;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Breakable Settings")]
    [SerializeField] private GameObject brokenVersion;
    [SerializeField] private float scoreAddAmount = 10f;

    private ScoreSystem scoreSystem;

    private void Start()
    {
        scoreSystem = GameObject.FindGameObjectWithTag("GameController").GetComponent<ScoreSystem>();

        if (promptText != null)
            promptText.text = "Press [E] to Break";
    }

    public void Interact(InteractionHandler interactor)
    {
        Debug.Log($"{gameObject.name} has been broken by {interactor.name}");

        if (scoreSystem != null)
            scoreSystem.AddScore(scoreAddAmount);

        if (brokenVersion != null)
            Instantiate(brokenVersion, transform.position, transform.rotation);

        Destroy(gameObject);
    }

    public void ShowPrompt()
    {
        if (promptUI != null) promptUI.SetActive(true);
    }

    public void HidePrompt()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }
}

