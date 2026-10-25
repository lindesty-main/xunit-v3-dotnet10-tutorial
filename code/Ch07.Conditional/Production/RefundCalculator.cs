namespace Ch07.Conditional.Production;

/// <summary>被测代码:退款计算(纯函数,unit 层测试直接调用)。</summary>
public static class RefundCalculator
{
    /// <summary>退款 = 单价 × 数量,但不超过实付金额。</summary>
    public static decimal Refund(decimal paid, decimal unitPrice, int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(quantity);
        return Math.Min(unitPrice * quantity, paid);
    }
}
