using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Type-based inventory and scoring. Duplicate core pickups award extra points; Money is always extra.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public enum ItemPickupKind
    {
        FirstCore,
        DuplicateCore,
        Extra
    }

    public static InventoryManager Instance { get; private set; }

    [Header("Scoring")]
    [SerializeField] private int corePointsPerItem = 100;
    [SerializeField] private int duplicateCoreExtraPoints = 100;
    [SerializeField] private int extraMoneyPoints = 150;

    public event Action<ItemType, ItemPickupKind, int> OnItemPickedUp;
    public event Action<int, int> OnScoreChanged;
    public event Action OnInventoryChanged;

    private readonly HashSet<ItemType> collectedCoreTypes = new HashSet<ItemType>();

    // MaxCoreScore is derived from a fixed enum, so compute it once and cache it.
    // This avoids iterating AllCoreTypes (which enumerates the entire enum) on every frame
    // that reads MaxCoreScore (e.g., for the score panel progress display).
    private int cachedMaxCoreScore = -1;

    public int CoreScore { get; private set; }
    public int ExtraScore { get; private set; }
    /// <summary>
    /// Maximum achievable core score (one entry per core ItemType × corePointsPerItem).
    /// Computed once and cached — the enum never changes at runtime.
    /// </summary>
    public int MaxCoreScore
    {
        get
        {
            if (cachedMaxCoreScore >= 0)
                return cachedMaxCoreScore;

            int count = 0;
            foreach (ItemType _ in ItemTypeCatalog.AllCoreTypes)
                count++;
            cachedMaxCoreScore = count * corePointsPerItem;
            return cachedMaxCoreScore;
        }
    }

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

    public bool IsCollected(ItemType itemType) =>
        ItemTypeCatalog.IsCore(itemType) && collectedCoreTypes.Contains(itemType);

    public bool HasAllCoreItems()
    {
        foreach (ItemType type in ItemTypeCatalog.AllCoreTypes)
        {
            if (!collectedCoreTypes.Contains(type))
                return false;
        }

        return true;
    }

    public int CollectedCoreCount => collectedCoreTypes.Count;

    public IReadOnlyCollection<ItemType> CollectedCoreTypes => collectedCoreTypes;

    /// <summary>
    /// Type-based item collection — NOT instance-based.
    /// Any <see cref="CollectibleItem"/> with the matching <see cref="ItemType"/> enum value
    /// is treated identically, regardless of which specific GameObject it comes from.
    /// This means placing 5 Water Bottles in the scene is fully supported:
    ///   - First pickup  → Core Score +100, checklist tick, robot first-core message.
    ///   - Each duplicate → Extra Score +100, robot duplicate message.
    ///   - Money (extra) → Extra Score +150, robot cash message, no checklist entry.
    /// Always returns true — callers can always destroy the world object after calling this.
    /// </summary>
    public bool TryCollectItem(ItemType itemType, out ItemPickupKind pickupKind, out int pointsAwarded)
    {
        pickupKind = ItemPickupKind.Extra;
        pointsAwarded = 0;

        if (ItemTypeCatalog.IsExtra(itemType))
        {
            pointsAwarded = extraMoneyPoints;
            ExtraScore += pointsAwarded;
            pickupKind = ItemPickupKind.Extra;

            RaisePickup(itemType, pickupKind, pointsAwarded);
            return true;
        }

        if (!collectedCoreTypes.Contains(itemType))
        {
            collectedCoreTypes.Add(itemType);
            pointsAwarded = corePointsPerItem;
            CoreScore += pointsAwarded;
            pickupKind = ItemPickupKind.FirstCore;

            RaisePickup(itemType, pickupKind, pointsAwarded);
            return true;
        }

        pointsAwarded = duplicateCoreExtraPoints;
        ExtraScore += pointsAwarded;
        pickupKind = ItemPickupKind.DuplicateCore;

        RaisePickup(itemType, pickupKind, pointsAwarded);
        return true;
    }

    private void RaisePickup(ItemType itemType, ItemPickupKind pickupKind, int pointsAwarded)
    {
        OnItemPickedUp?.Invoke(itemType, pickupKind, pointsAwarded);
        OnScoreChanged?.Invoke(CoreScore, ExtraScore);
        OnInventoryChanged?.Invoke();
    }

    public static string GetDisplayName(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.WaterBottle => "Water Bottle",
            ItemType.Flashlight => "Flashlight",
            ItemType.FirstAidKit => "First Aid Kit",
            ItemType.Whistle => "Whistle",
            ItemType.Radio => "Battery-powered Radio",
            ItemType.Food => "Canned Food",
            ItemType.Knife => "Pocket Knife",
            ItemType.Documents => "Important Documents Folder",
            ItemType.Money => "Money / Cash",
            _ => itemType.ToString()
        };
    }
}
