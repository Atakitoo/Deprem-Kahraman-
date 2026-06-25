using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// SubtitleManager — Singleton UI controller for in-game subtitles.
///
/// SETUP (Inspector):
///   1. Create a Canvas → rename it "SubtitleCanvas".
///   2. Add a child Panel → rename it "SubtitlePanel".
///      - Anchor: Bottom-Center, stretch horizontally.
///      - Image color: (0, 0, 0, 0.55) for a semi-transparent bar.
///   3. Add a child TextMeshPro - Text (UI) inside SubtitlePanel → rename it "SubtitleText".
///      - Alignment: Center/Middle, Color: White, Font Size: ~32.
///   4. Attach THIS script to "SubtitleCanvas" (or any persistent GameObject).
///   5. Drag "SubtitlePanel" into the [SubtitlePanel] slot.
///   6. Drag "SubtitleText" into the [SubtitleText] slot.
/// </summary>
public class SubtitleManager : MonoBehaviour
{
    // ─── Singleton ────────────────────────────────────────────────────────────

    /// <summary>Global access point. No Inspector drag-and-drop needed from other scripts.</summary>
    public static SubtitleManager Instance { get; private set; }

    // ─── Inspector References ─────────────────────────────────────────────────

    [Header("UI References")]
    [Tooltip("The root Panel GameObject that wraps the subtitle bar. Will be toggled on/off.")]
    [SerializeField] private GameObject subtitlePanel;

    [Tooltip("The TextMeshPro component that displays the subtitle text.")]
    [SerializeField] private TMP_Text subtitleText;

    // ─── Private State ────────────────────────────────────────────────────────

    /// <summary>Keeps track of the currently running coroutine so it can be stopped
    /// if a new subtitle is requested before the old one expires.</summary>
    private Coroutine _activeCoroutine;

    // ─── Unity Lifecycle ──────────────────────────────────────────────────────

    private void Awake()
    {
        // Enforce a single instance across all scenes.
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[SubtitleManager] Duplicate instance detected — destroying this copy.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Optional: keep this manager alive when loading new scenes.
        // Remove this line if your Canvas lives only in one scene.
        DontDestroyOnLoad(gameObject);

        // Safety: hide the panel on startup so it doesn't appear before any subtitle is shown.
        HidePanel();
    }

    // ─── Public API ───────────────────────────────────────────────────────────

    /// <summary>
    /// Displays a subtitle on screen for the given duration.
    /// Safe to call while another subtitle is already showing — the old one
    /// is cancelled and the new one replaces it immediately.
    /// </summary>
    /// <param name="text">The message to display.</param>
    /// <param name="duration">How long (in seconds) the subtitle stays visible.</param>
    public void DisplaySubtitle(string text, float duration)
    {
        // Validate references before doing anything.
        if (subtitlePanel == null || subtitleText == null)
        {
            Debug.LogError("[SubtitleManager] SubtitlePanel or SubtitleText reference is missing! " +
                           "Please assign them in the Inspector.");
            return;
        }

        // If a subtitle is already running, stop it cleanly before starting the new one.
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }

        // Start the new subtitle coroutine and cache its handle.
        _activeCoroutine = StartCoroutine(ShowSubtitleRoutine(text, duration));
    }

    /// <summary>
    /// Immediately hides the subtitle panel regardless of any running timer.
    /// Useful for cutscene skips, death screens, or scene transitions.
    /// </summary>
    public void HideSubtitleImmediately()
    {
        if (_activeCoroutine != null)
        {
            StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }

        HidePanel();
    }

    // ─── Private Helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Coroutine that handles the full subtitle lifecycle:
    /// show → wait → hide.
    /// </summary>
    private IEnumerator ShowSubtitleRoutine(string text, float duration)
    {
        // 1. Show the panel and set the text.
        subtitlePanel.SetActive(true);
        subtitleText.text = text;

        // 2. Wait for the requested duration.
        yield return new WaitForSeconds(duration);

        // 3. Duration elapsed with no interruption — clear and hide.
        HidePanel();

        // Null out the handle since the coroutine finished naturally.
        _activeCoroutine = null;
    }

    /// <summary>Clears the text and deactivates the panel.</summary>
    private void HidePanel()
    {
        if (subtitleText != null)
            subtitleText.text = string.Empty;

        if (subtitlePanel != null)
            subtitlePanel.SetActive(false);
    }
}
