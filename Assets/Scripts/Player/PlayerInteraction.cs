using TMPro;
using UnityEngine;

/// <summary>
/// Screen-center raycast interaction. Left-click picks up collectibles, uses the bed, or activates IInteractables.
/// Attach to the Main Camera in gameplay scenes.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private Camera raycastCamera;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private LayerMask interactableLayers = ~0;
    [SerializeField] private bool useScreenCenterRay = true;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private bool allowInteractKeyForWorldObjects = true;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private bool autoBindHudOnAwake = true;

    private GameHUD boundHud;
    private bool interactionEnabled = true;

    public bool InteractionEnabled
    {
        get => interactionEnabled;
        set
        {
            interactionEnabled = value;
            if (!interactionEnabled)
                ClearPrompt();
        }
    }

    private void Awake()
    {
        if (raycastCamera == null)
            raycastCamera = GetComponent<Camera>();

        if (raycastCamera == null)
            raycastCamera = Camera.main;

        if (autoBindHudOnAwake)
            TryBindHudFromScene();
    }

    private void Start()
    {
        // BUG 1 FIX: Attempt a late HUD bind. If BeforeSceneManager instantiated a HUD prefab
        // during its own Awake(), GameHUD.Instance wasn't set yet when our Awake() ran above.
        // Start() is guaranteed to fire after ALL Awake() calls in the scene are done,
        // so the spawned instance is registered and findable by this point.
        if (boundHud == null)
            TryBindHudFromScene();
    }

    public void BindHud(GameHUD hud)
    {
        boundHud = hud;

        if (promptText == null && hud != null)
            promptText = hud.InteractPromptText;
    }

    /// <summary>
    /// Finds the GameHUD singleton (or first scene instance) and binds to it.
    /// Public so BeforeSceneManager can force a re-bind after spawning a HUD prefab.
    /// Safe to call multiple times — exits immediately if already bound.
    /// </summary>
    public void TryBindHudFromScene()
    {
        // BUG 1 FIX: Original guard was `if (promptText != null) return`, which returned
        // early whenever promptText was assigned in the Inspector — even when boundHud was
        // still null. This prevented runtime-spawned HUDs from ever being registered.
        // Correct guard: only skip when the HUD itself is already bound.
        if (boundHud != null)
            return;

        GameHUD hud = GameHUD.Instance ?? FindFirstObjectByType<GameHUD>();
        if (hud != null)
            BindHud(hud);
    }

    private void Update()
    {
        if (!interactionEnabled)
        {
            ClearPrompt();
            return;
        }

        UpdateHoverPrompt();

        if (WasPrimaryInteractPressed())
            TryInteract();
    }

    private void UpdateHoverPrompt()
    {
        if (!TryGetRaycastHit(out RaycastHit hit))
        {
            ClearPrompt();
            return;
        }

        CollectibleItem collectible = hit.collider.GetComponentInParent<CollectibleItem>();
        if (collectible != null)
        {
            SetPrompt(collectible.PickupPrompt);
            return;
        }

        BedInteractable bed = hit.collider.GetComponentInParent<BedInteractable>();
        if (bed != null)
        {
            bool canSleep = BeforeSceneManager.Instance != null && BeforeSceneManager.Instance.CanUseBed();
            SetPrompt(canSleep ? bed.PromptWhenReady : bed.PromptWhenLocked);
            return;
        }

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable != null && interactable.CanInteract(gameObject))
        {
            SetPrompt(interactable.InteractionPrompt);
            return;
        }

        ClearPrompt();
    }

    private void TryInteract()
    {
        if (!TryGetRaycastHit(out RaycastHit hit))
            return;

        CollectibleItem collectible = hit.collider.GetComponentInParent<CollectibleItem>();
        if (collectible != null)
        {
            collectible.TryPickup();
            return;
        }

        BedInteractable bed = hit.collider.GetComponentInParent<BedInteractable>();
        if (bed != null)
        {
            if (BeforeSceneManager.Instance != null)
                BeforeSceneManager.Instance.TryInteractWithBed();
            else
                Debug.LogWarning("BedInteractable requires BeforeSceneManager in the scene.");

            return;
        }

        IInteractable interactable = hit.collider.GetComponentInParent<IInteractable>();
        if (interactable != null && interactable.CanInteract(gameObject))
            interactable.Interact(gameObject);
    }

    private bool TryGetRaycastHit(out RaycastHit hit)
    {
        hit = default;

        if (raycastCamera == null)
            return false;

        Ray ray = useScreenCenterRay
            ? raycastCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f))
            : raycastCamera.ScreenPointToRay(Input.mousePosition);

        return Physics.Raycast(
            ray,
            out hit,
            interactDistance,
            interactableLayers,
            QueryTriggerInteraction.Collide);
    }

    private bool WasPrimaryInteractPressed()
    {
        if (Input.GetMouseButtonDown(0))
            return true;

        return allowInteractKeyForWorldObjects && Input.GetKeyDown(interactKey);
    }

    private void SetPrompt(string message)
    {
        if (boundHud != null)
        {
            boundHud.SetInteractPrompt(message);
            return;
        }

        if (promptText != null)
            promptText.text = message;
    }

    private void ClearPrompt()
    {
        if (boundHud != null)
        {
            boundHud.SetInteractPrompt(string.Empty);
            return;
        }

        if (promptText != null)
            promptText.text = string.Empty;
    }
}
