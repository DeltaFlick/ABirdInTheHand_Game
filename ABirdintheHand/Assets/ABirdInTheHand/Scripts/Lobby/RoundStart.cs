using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Controls the round start process with player teleportation and countdown
/// </summary>
public class RoundStart : MonoBehaviour
{
    [Header("Teleport Settings (one per player, ordered by index)")]
    [SerializeField] private GameObject[] teleportDestinations;

    [Header("Timer Reference")]
    [SerializeField] private Timer timer;

    [Header("UI Elements")]
    [SerializeField] private GameObject scoreTextObject;
    [SerializeField] private GameObject timerTextObject;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Countdown Settings")]
    [SerializeField] private float countdownDuration = 3f;

    // Round state
    private int playersInTrigger = 0;
    private bool startRoundActive = false;
    private Coroutine countdownCoroutine;
    private HashSet<GameObject> teleportedPlayers = new HashSet<GameObject>();
    private PlayerInputManager playerInputManager;

    // Cached values
    private WaitForSeconds oneSecondWait;
    private WaitForSeconds shortDelayWait;

    private void Awake()
    {
        playerInputManager = FindObjectOfType<PlayerInputManager>();

        if (playerInputManager == null)
        {
            Debug.LogWarning("[RoundStart] PlayerInputManager not found in scene", this);
        }

        oneSecondWait = new WaitForSeconds(1f);
        shortDelayWait = new WaitForSeconds(0.1f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playersInTrigger++;
        CheckStartCondition();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playersInTrigger = Mathf.Max(0, playersInTrigger - 1);

        int totalPlayers = PlayerInput.all.Count;
        if (startRoundActive && playersInTrigger < totalPlayers)
        {
            CancelCountdown();
        }
    }

    private void CheckStartCondition()
    {
        int totalPlayers = PlayerInput.all.Count;
        if (totalPlayers == 0) return;

        if (playersInTrigger == totalPlayers && !startRoundActive)
        {
            countdownCoroutine = StartCoroutine(StartRound());
        }
    }

    private void CancelCountdown()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }

        startRoundActive = false;

        if (countdownText != null)
        {
            countdownText.text = string.Empty;
            countdownText.gameObject.SetActive(false);
        }

        Debug.Log("[RoundStart] Countdown cancelled — not all players in trigger.");
    }

    private IEnumerator StartRound()
    {
        startRoundActive = true;

        float countdown = countdownDuration;
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
        }

        int totalPlayers = PlayerInput.all.Count;

        while (countdown > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.Ceil(countdown).ToString();
            }

            yield return oneSecondWait;
            countdown -= 1f;

            if (playersInTrigger < totalPlayers)
            {
                CancelCountdown();
                yield break;
            }
        }

        if (countdownText != null)
        {
            countdownText.text = "GO!";
            yield return oneSecondWait;
            countdownText.gameObject.SetActive(false);
        }

        TeleportPlayers();

        playersInTrigger = 0;

        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.SetScoreUIVisible(true);
            ScoreSystem.Instance.SetScore(0f);
        }
        else
        {
            Debug.LogWarning("[RoundStart] ScoreSystem instance not found", this);
        }

        if (timerTextObject != null)
        {
            timerTextObject.SetActive(true);
        }

        if (timer != null)
        {
            timer.ResetTimer(300f);
        }

        if (playerInputManager != null)
        {
            playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersManually;
        }

        teleportedPlayers.Clear();
        startRoundActive = false;
        countdownCoroutine = null;

        Debug.Log("[RoundStart] Round successfully started!");
    }

    private void TeleportPlayers()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            if (player == null || teleportedPlayers.Contains(player))
                continue;

            PlayerInput input = player.GetComponent<PlayerInput>();
            if (input == null) continue;

            int index = input.playerIndex;
            if (index >= teleportDestinations.Length)
            {
                Debug.LogWarning($"[RoundStart] No teleport destination for player index {index}", this);
                continue;
            }

            Transform destination = teleportDestinations[index].transform;

            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.MovePosition(destination.position);
                rb.MoveRotation(destination.rotation);
            }
            else
            {
                player.transform.SetPositionAndRotation(destination.position, destination.rotation);
            }

            teleportedPlayers.Add(player);

            PlayerControls pc = player.GetComponent<PlayerControls>();
            if (pc != null)
            {
                pc.enabled = false;
                StartCoroutine(ReenableControls(pc));
            }

            PlayerMenuController menuController = player.GetComponent<PlayerMenuController>();
            if (menuController != null)
            {
                menuController.SetRoundStarted(true);
            }
        }
    }

    private IEnumerator ReenableControls(PlayerControls pc)
    {
        yield return shortDelayWait;

        if (pc != null)
        {
            pc.enabled = true;
        }
    }

    public void EndRound()
    {
        if (ScoreSystem.Instance != null)
        {
            ScoreSystem.Instance.SetScoreUIVisible(false);
            ScoreSystem.Instance.SetScore(0f);
        }

        if (timerTextObject != null)
        {
            timerTextObject.SetActive(false);
        }

        if (playerInputManager != null)
        {
            playerInputManager.joinBehavior = PlayerJoinBehavior.JoinPlayersWhenButtonIsPressed;
        }

        Debug.Log("[RoundStart] Round ended and UI hidden.");
    }

    private void OnDestroy()
    {
        if (countdownCoroutine != null)
        {
            StopCoroutine(countdownCoroutine);
            countdownCoroutine = null;
        }
    }
}