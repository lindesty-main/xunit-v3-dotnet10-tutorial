namespace Ch08.Timeouts.Production;

/// <summary>一条库存台账记录:SKU 与实际数量。</summary>
public sealed record LedgerEntry(string Sku, int Quantity);
