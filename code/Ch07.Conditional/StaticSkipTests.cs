using Ch07.Conditional.Production;

namespace Ch07.Conditional.Tests;

/// <summary>
/// 7.1 静态跳过:原因写在特性上,发现阶段就决定不执行。
/// </summary>
[Trait("kind", "unit")]
[Trait("speed", "fast")]
public class StaticSkipTests
{
    [Fact]
    public void Refund_CapsAtPaid() =>
        Assert.Equal(30.00m, RefundCalculator.Refund(30.00m, 12.00m, 5));

    /// <summary>特性级 Skip:整条测试不参与执行,原因原样进报告。</summary>
    [Fact(Skip = "部分退货的退款上限规则未定,见需求 #1427")]
    public void Refund_PartialShipment_PendingRule() =>
        Assert.Equal(6.00m, RefundCalculator.Refund(30.00m, 12.00m, 1));

    /// <summary>数据行级 Skip:同一个 Theory 里别的行照常跑。</summary>
    [Theory]
    [InlineData(12.00, 1, 12.00)]
    [InlineData(12.00, 3, 36.00)]
    [InlineData(12.00, 9, 100.00, Skip = "实付封顶的边界用例,与 #1427 一起再启用")]
    public void Refund_QuantityTable(double unitPrice, int quantity, double expected) =>
        Assert.Equal((decimal)expected, RefundCalculator.Refund(100.00m, (decimal)unitPrice, quantity));
}
