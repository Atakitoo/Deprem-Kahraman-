using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// House exploration phase: scoring, bed gate, robot education routing, transition handoff.
/// </summary>
public class BeforeSceneManager : MonoBehaviour
{
    public static BeforeSceneManager Instance { get; private set; }

    [Header("HUD")]
    [SerializeField] private GameHUDBootstrap hudBootstrap;
    [SerializeField] private GameHUD gameHud;
    // BUG 1 FIX: Assign the HUD Canvas Prefab here. If the Before scene has no GameHUD
    // instance in its hierarchy, BeforeSceneManager will instantiate this prefab automatically
    // at runtime — no need to place the HUD in every scene manually.
    [SerializeField] private GameObject hudPrefab;

    [Header("References")]
    [SerializeField] private InventoryManager inventory;
    [SerializeField] private PhoneAppManager phoneAppManager;
    [SerializeField] private TransitionScreenManager transitionScreenManager;

    [Header("Player Freeze")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private PlayerInteraction playerInteraction;

    [Header("Bed Gate")]
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private string bedLockedMessage =
        "I should finish gathering my core emergency items before going to sleep.";
    [SerializeField] private float subtitleDisplayDuration = 4f;

    private bool transitionStarted;

    public event Action OnBedInteractionAccepted;
    public event Action OnBedInteractionRejected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (inventory == null)
            inventory = InventoryManager.Instance;

        // Resolve the HUD reference now so subtitleText is available for Start().
        // NOTE: WirePlayerInteractionToHud() is intentionally deferred to Start().
        // Calling it here in Awake() is a timing bug: if the HUD was just instantiated
        // from a prefab in ResolveHudReferences(), its GameHUD.Instance singleton might
        // not be registered yet when PlayerInteraction tries to bind to it.
        ResolveHudReferences();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void OnEnable()
    {
        if (inventory != null)
            inventory.OnItemPickedUp += HandleItemPickedUp;
    }

    private void OnDisable()
    {
        if (inventory != null)
            inventory.OnItemPickedUp -= HandleItemPickedUp;
    }

    private void Start()
    {
        if (subtitleText != null)
            subtitleText.gameObject.SetActive(false);

        // BUG 1 FIX: Wire PlayerInteraction to the HUD here in Start(), not in Awake().
        // By the time Start() runs, all Awake() calls have completed, so any HUD spawned
        // from a prefab (in ResolveHudReferences) is fully initialized and its singleton
        // is registered. This guarantees the interact-prompt text is correctly bound.
        WirePlayerInteractionToHud();
    }

    public bool CanUseBed()
    {
        return inventory != null && inventory.HasAllCoreItems();
    }

    public void TryInteractWithBed()
    {
        if (transitionStarted)
            return;

        if (!CanUseBed())
        {
            OnBedInteractionRejected?.Invoke();
            ShowTemporarySubtitle(bedLockedMessage);
            return;
        }

        transitionStarted = true;
        OnBedInteractionAccepted?.Invoke();
        FreezePlayer(true);

        if (transitionScreenManager != null)
        {
            transitionScreenManager.BeginTransition(inventory.CoreScore, inventory.ExtraScore);
            return;
        }

        Debug.LogError("BeforeSceneManager: TransitionScreenManager is not assigned.");
    }

    private void HandleItemPickedUp(ItemType itemType, InventoryManager.ItemPickupKind pickupKind, int pointsAwarded)
    {
        if (phoneAppManager == null)
            return;

        switch (pickupKind)
        {
            case InventoryManager.ItemPickupKind.FirstCore:
                phoneAppManager.AppendRobotMessage(ItemEducationMessages.GetFirstCoreMessage(itemType));
                break;
            case InventoryManager.ItemPickupKind.DuplicateCore:
                phoneAppManager.AppendRobotMessage(ItemEducationMessages.GetDuplicateCoreMessage(itemType));
                break;
            case InventoryManager.ItemPickupKind.Extra:
                phoneAppManager.AppendRobotMessage(ItemEducationMessages.GetExtraMessage(itemType));
                break;
        }
    }

    private void ResolveHudReferences()
    {
        // Step 1: Prefer an explicitly assigned HUD or one managed by the bootstrap.
        if (gameHud == null && hudBootstrap != null)
            gameHud = hudBootstrap.EnsureCreated();

        // Step 2: Fall back to the scene-wide singleton.
        if (gameHud == null)
            gameHud = GameHUD.Instance;

        // Step 3: Broad scene search as a last resort before prefab spawning.
        if (gameHud == null)
            gameHud = FindFirstObjectByType<GameHUD>();

        // Step 4 (BUG 1 FIX): No HUD found anywhere — instantiate from the assigned prefab.
        // This is the core fix: the Before scene doesn't need the HUD Canvas manually placed
        // in its hierarchy. Assign the prefab in the Inspector and it will spawn here.
        if (gameHud == null && hudPrefab != null)
        {
            GameObject instance = Instantiate(hudPrefab);
            instance.name = hudPrefab.name;
            gameHud = instance.GetComponent<GameHUD>();

            if (gameHud == null)
                Debug.LogError("BeforeSceneManager: The assigned HUD Prefab has no GameHUD component!");
            else
                Debug.Log("BeforeSceneManager: GameHUD instantiated from prefab automatically.");
        }

        if (gameHud == null)
            Debug.LogWarning("BeforeSceneManager: No GameHUD found and no prefab assigned. " +
                             "Assign the HUD Prefab field or place a GameHUD in the scene.");

        // Resolve subtitle text from the HUD if not manually overridden in the Inspector.
        if (subtitleText == null && gameHud != null)
            subtitleText = gameHud.SubtitleText;
    }

    private void WirePlayerInteractionToHud()
    {
        if (playerInteraction == null)
            playerInteraction = FindFirstObjectByType<PlayerInteraction>();

        if (playerInteraction != null && gameHud != null)
            playerInteraction.BindHud(gameHud);
    }

    private void ShowTemporarySubtitle(string message)
    {
        if (subtitleText == null)
        {
            Debug.LogWarning($"BeforeSceneManager: {message}");
            return;
        }

        StopAllCoroutines();
        StartCoroutine(ShowSubtitleRoutine(message));
    }

    private IEnumerator ShowSubtitleRoutine(string message)
    {
        subtitleText.text = message;
        subtitleText.gameObject.SetActive(true);
        yield return new WaitForSeconds(subtitleDisplayDuration);
        subtitleText.gameObject.SetActive(false);
    }

    public void FreezePlayer(bool freeze)
    {
        if (playerMovement != null)
            playerMovement.MovementEnabled = !freeze;

        if (playerLook != null)
            playerLook.LookEnabled = !freeze;

        if (playerInteraction != null)
            playerInteraction.InteractionEnabled = !freeze;
    }
}
