using System;
using System.Collections.Generic;

/// <summary>
/// Helpers for distinguishing core checklist items from extra items like Money.
/// </summary>
public static class ItemTypeCatalog
{
    public static bool IsCore(ItemType itemType) => itemType != ItemType.Money;

    public static bool IsExtra(ItemType itemType) => itemType == ItemType.Money;

    public static IEnumerable<ItemType> AllCoreTypes
    {
        get
        {
            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                if (IsCore(type))
                    yield return type;
            }
        }
    }
}
