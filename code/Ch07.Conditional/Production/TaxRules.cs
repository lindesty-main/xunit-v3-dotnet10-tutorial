namespace Ch07.Conditional.Production;

/// <summary>被测代码:区域 → 税则区 的映射表。表里没有的区域不参与校验。</summary>
public static class TaxRules
{
    private static readonly Dictionary<string, string> Zones = new(StringComparer.OrdinalIgnoreCase)
    {
        ["DE"] = "EU",
        ["FR"] = "EU",
        ["NO"] = "NO",
    };

    public static bool HasRule(string region) => Zones.ContainsKey(region);

    public static string ZoneOf(string region) => Zones[region];
}
