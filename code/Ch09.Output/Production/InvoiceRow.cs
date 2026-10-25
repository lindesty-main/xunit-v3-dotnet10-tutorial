namespace Ch09.Output.Production;

/// <summary>一条待对账的发票行。<c>Id</c> 用来在台账里定位对应金额。</summary>
public sealed record InvoiceRow(string Id, string Vendor, decimal Amount);
