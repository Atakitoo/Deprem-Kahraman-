/// <summary>
/// Robot assistant educational copy for each collectible type.
/// </summary>
public static class ItemEducationMessages
{
    public static string GetFirstCoreMessage(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.WaterBottle =>
                "RoBee: Temiz su hayati önem taşır. Depremden sonra musluklar güvenli olmayabilir veya akmayabilir.",
            ItemType.Flashlight =>
                "RoBee: Afetlerden sonra elektrik kesintileri yaygındır. El feneri, karanlıkta güvenli bir şekilde hareket etmenize yardımcı olur",
            ItemType.FirstAidKit =>
                "RoBee: İlk yardım çantası, profesyonel yardım gelene kadar küçük yaralanmalara müdahale eder.",
            ItemType.Whistle =>
                "RoBee: Düdük, sesinizi tüketmeden kurtarma ekiplerinin sizi bulmasına yardımcı olur.",
            ItemType.Radio =>
                "RoBee: Pille çalışan bir radyo, telefonlar ve internet devre dışı kaldığında haberleri almanızı sağlar",
            ItemType.Food =>
                "RoBee: Mağazalar kapalı kalırsa, bozulmayan gıdalar enerjinizi yüksek tutar.",
            ItemType.Knife =>
                "RoBee: Küçük bir alet; paketleri açabilir, bantları kesebilir veya basit onarımlarda yardımcı olabilir.",
            ItemType.Documents =>
                "RoBee: Kimlik ve sigorta belgelerinin kopyaları, bir felaket sonrasındaki toparlanma sürecini hızlandırır.",
            _ => $"ROBOT: {InventoryManager.GetDisplayName(itemType)} added to your emergency kit."
        };
    }

    public static string GetDuplicateCoreMessage(ItemType itemType)
    {
        return $"ROBOT: You already packed a {InventoryManager.GetDisplayName(itemType)}. " +
               "Extra copies earn bonus preparedness points, but keep your real emergency bag lightweight!";
    }

    public static string GetExtraMessage(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.Money =>
                "ROBOT: Cash is vital because ATMs and card systems might not work during power outages after a disaster.",
            _ => $"ROBOT: Extra preparedness item collected: {InventoryManager.GetDisplayName(itemType)}."
        };
    }
}
