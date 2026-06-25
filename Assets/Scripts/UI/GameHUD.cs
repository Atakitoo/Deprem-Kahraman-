using TMPro;
using UnityEngine;

/// <summary>
/// Shared gameplay HUD: subtitles, interact prompt, score readout.
/// Place once per scene or spawn from the GameHUD prefab.
/// </summary>
public class GameHUD : MonoBehaviour
{
    public static GameHUD Instance { get; private set; }

    [Header("Text Elements")]
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private TMP_Text interactPromptText;
    [SerializeField] private TMP_Text coreScoreText;
    [SerializeField] private TMP_Text extraScoreText;

    [Header("Optional Panels")]
    [SerializeField] private GameObject subtitlePanel;
    [SerializeField] private GameObject scorePanel;

    public TMP_Text SubtitleText => subtitleText;
    public TMP_Text InteractPromptText => interactPromptText;
    public TMP_Text CoreScoreText => coreScoreText;
    public TMP_Text ExtraScoreText => extraScoreText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnScoreChanged += HandleScoreChanged;
    }

    private void Start()
    {
        if (subtitleText != null)
            subtitleText.gameObject.SetActive(false);

        if (InventoryManager.Instance != null)
            HandleScoreChanged(InventoryManager.Instance.CoreScore, InventoryManager.Instance.ExtraScore);
        else
            UpdateScoreDisplay(0, 0);
    }

    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnScoreChanged -= HandleScoreChanged;
    }

    public void SetInteractPrompt(string message)
    {
        if (interactPromptText == null)
            return;

        interactPromptText.text = message;
        interactPromptText.gameObject.SetActive(!string.IsNullOrEmpty(message));
    }

    public void UpdateScoreDisplay(int coreScore, int extraScore)
    {
        if (coreScoreText != null)
            coreScoreText.text = $"Core: {coreScore}";

        if (extraScoreText != null)
            extraScoreText.text = $"Extra: {extraScore}";

        if (scorePanel != null)
            scorePanel.SetActive(coreScoreText != null || extraScoreText != null);
    }

    private void HandleScoreChanged(int coreScore, int extraScore)
    {
        UpdateScoreDisplay(coreScore, extraScore);
    }
}
