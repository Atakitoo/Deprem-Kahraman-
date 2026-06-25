using TMPro;
using UnityEngine;

/// <summary>
/// Ensures a GameHUD exists in the scene — uses an assigned instance or spawns the prefab.
/// Attach to GameSystems in Tutorial and Before scenes.
/// </summary>
public class GameHUDBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject hudPrefab;
    [SerializeField] private GameHUD existingHud;
    [SerializeField] private bool spawnIfMissing = true;

    public GameHUD HUD { get; private set; }

    private void Awake()
    {
        EnsureCreated();
    }

    public GameHUD EnsureCreated()
    {
        if (HUD != null)
            return HUD;

        HUD = ResolveHUD();

        if (HUD == null && spawnIfMissing && hudPrefab != null)
        {
            GameObject instance = Instantiate(hudPrefab);
            instance.name = hudPrefab.name;
            HUD = instance.GetComponent<GameHUD>();

            if (HUD == null)
                Debug.LogError("GameHUDBootstrap: HUD prefab is missing the GameHUD component.");
        }

        if (HUD == null)
            Debug.LogWarning("GameHUDBootstrap: No GameHUD found. Assign a prefab or place GameHUD in the scene.");

        return HUD;
    }

    private GameHUD ResolveHUD()
    {
        if (existingHud != null)
            return existingHud;

        if (GameHUD.Instance != null)
            return GameHUD.Instance;

        return FindFirstObjectByType<GameHUD>();
    }
}
