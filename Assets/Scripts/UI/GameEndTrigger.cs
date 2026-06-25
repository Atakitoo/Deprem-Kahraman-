using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// GameEndTrigger — Attach to a GameObject with a Box Collider (Is Trigger = true).
///
/// When the player enters this trigger zone:
///   1. Freezes player movement and camera look.
///   2. Unlocks and shows the mouse cursor.
///   3. Activates the Final Victory Canvas.
///
/// INSPECTOR SETUP:
///   • Tag your Player GameObject as "Player".
///   • Assign [finalVictoryCanvas] → your FinalVictoryCanvas GameObject.
///   • [mainMenuSceneName] → exact name of your main menu scene (e.g. "MainMenu").
///   • The script auto-finds PlayerMovement, PlayerLook, and PlayerInteraction
///     on the Player at runtime — no drag-and-drop needed for those.
/// </summary>
public class GameEndTrigger : MonoBehaviour
{
    // ─── Inspector Fields ─────────────────────────────────────────────────────

    [Header("Victory UI")]
    [Tooltip("The FinalVictoryCanvas GameObject. Must be inactive by default.")]
    [SerializeField] private GameObject finalVictoryCanvas;

    [Header("Scene Transition")]
    [Tooltip("Exact name of the Main Menu scene as it appears in Build Settings.")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    [Header("Trigger Settings")]
    [Tooltip("Only GameObjects with this tag will trigger the ending.")]
    [SerializeField] private string playerTag = "Player";

    // ─── Private State ────────────────────────────────────────────────────────

    private bool _hasTriggered = false;

    // ─── Trigger Detection ────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        // Guard: only fire once, and only for the tagged player.
        if (_hasTriggered || !other.CompareTag(playerTag))
            return;

        _hasTriggered = true;

        FreezePlayer(other.gameObject);
        ShowVictoryScreen();
    }

    // ─── Player Freeze ────────────────────────────────────────────────────────

    /// <summary>
    /// Disables movement, camera look, and interaction on the player.
    /// Uses the exact property names from PlayerMovement, PlayerLook,
    /// and PlayerInteraction — safe to call if any component is missing.
    /// </summary>
    private void FreezePlayer(GameObject player)
    {
        // Disable body movement (PlayerMovement.cs — MovementEnabled property).
        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        if (movement != null)
            movement.MovementEnabled = false;

        // Disable camera rotation and unlock the cursor (PlayerLook.cs).
        PlayerLook look = player.GetComponentInChildren<PlayerLook>();
        if (look != null)
        {
            look.LookEnabled = false;

            // Use the dedicated helper so it stays in sync with PlayerLook's internal state.
            look.SetCursorLocked(false);
        }
        else
        {
            // Fallback: unlock cursor directly if PlayerLook isn't found.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // Disable item interaction (PlayerInteraction.cs — InteractionEnabled property).
        PlayerInteraction interaction = player.GetComponentInChildren<PlayerInteraction>();
        if (interaction != null)
            interaction.InteractionEnabled = false;
    }

    // ─── Victory Screen ───────────────────────────────────────────────────────

    /// <summary>Activates the victory canvas. Logs a clear warning if it wasn't assigned.</summary>
    private void ShowVictoryScreen()
    {
        if (finalVictoryCanvas == null)
        {
            Debug.LogError("[GameEndTrigger] 'Final Victory Canvas' is not assigned in the Inspector!");
            return;
        }

        finalVictoryCanvas.SetActive(true);
    }

    // ─── Public Button Callback ───────────────────────────────────────────────

    /// <summary>
    /// Called by the "Return to Main Menu" Button's OnClick() event.
    /// Loads the main menu scene asynchronously.
    /// </summary>
    public void ReturnToMainMenu()
    {
        StartCoroutine(LoadMainMenuAsync());
    }

    private IEnumerator LoadMainMenuAsync()
    {
        // Optional: you could activate a loading screen here before yielding.
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mainMenuSceneName);

        // Wait until the scene is fully loaded before switching.
        while (!asyncLoad.isDone)
            yield return null;
    }
}
