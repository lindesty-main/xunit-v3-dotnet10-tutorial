namespace Ch09.Output.Production;

/// <summary>
/// 一组固定的对账样本:三条发票对三行台账。
/// INV-1001 一致;INV-1002 金额漂移;INV-1003 在台账里不存在;INV-1004 是台账里多出的行。
/// 因此 <see cref="InvoiceReconciler.FindMismatchedRows" /> 正常应返回 2 行(INV-1002、INV-1003)。
/// </summary>
public static class InvoiceBook
{
    public static readonly InvoiceRow[] Invoices =
    [
        new("INV-1001", "Fabrikam", 120.00m),
        new("INV-1002", "Contoso", 88.50m),
        new("INV-1003", "Litware", 42.00m),
    ];

    public static readonly Dictionary<string, decimal> Ledger = new()
    {
        ["INV-1001"] = 120.00m,
        ["INV-1002"] = 90.00m,
        ["INV-1004"] = 7.25m,
    };
}
