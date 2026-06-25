using UnityEngine;

/// <summary>
/// World pickup collected via left-click raycast. Type is selected in the Inspector dropdown.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CollectibleItem : MonoBehaviour
{
    [SerializeField] private ItemType itemType = ItemType.WaterBottle;
    [SerializeField] private bool destroyOnCollect = true;
    [SerializeField] private GameObject pickupVfxPrefab;

    public ItemType ItemType => itemType;

    // BUG 2 FIX: Original ternary had identical strings on both branches — the IsExtra check
    // was evaluated but its result was discarded. Now Extra items show a "[Bonus]" prefix so
    // the player can see at a glance that Money is a bonus pickup, not a checklist item.
    public string PickupPrompt =>
        ItemTypeCatalog.IsExtra(itemType)
            ? $"[Bonus] Pick up {InventoryManager.GetDisplayName(itemType)}"
            : $"Pick up {InventoryManager.GetDisplayName(itemType)}";

    private void Reset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = false;
    }

    /// <summary>Called by PlayerInteraction when the player left-clicks this object.</summary>
    public bool TryPickup()
    {
        InventoryManager inventory = InventoryManager.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("CollectibleItem: No InventoryManager in scene.");
            return false;
        }

        if (!inventory.TryCollectItem(itemType, out _, out _))
            return false;

        if (pickupVfxPrefab != null)
            Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);

        if (destroyOnCollect)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);

        return true;
    }
}
