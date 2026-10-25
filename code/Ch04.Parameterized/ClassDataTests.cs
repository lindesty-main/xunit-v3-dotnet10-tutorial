using System.Collections;
using Ch04.Parameterized.Production;

namespace Ch04.Parameterized.Tests;

/// <summary>
/// 可复用的数据类:一份坏输入表,两个测试类都要用(一个测 TryParse,一个测 Parse 的异常路径)。
/// 首选 IEnumerable&lt;TheoryDataRow&lt;...&gt;&gt;:类型有检查,还能带元数据。
/// </summary>
public sealed class InvalidTrackingNumberData : IEnumerable<TheoryDataRow<string?>>
{
    public IEnumerator<TheoryDataRow<string?>> GetEnumerator()
    {
        yield return new TheoryDataRow<string?>(null) { TestDisplayName = "空引用" };
        yield return new TheoryDataRow<string?>("") { TestDisplayName = "空字符串" };
        yield return new TheoryDataRow<string?>("   ") { TestDisplayName = "只有空白" };
        yield return new TheoryDataRow<string?>("SF12345678901") { TestDisplayName = "少一位" };
        yield return new TheoryDataRow<string?>("AB123456789012") { TestDisplayName = "未知承运商" };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>旧式写法:实现 IEnumerable&lt;object[]&gt;。能用,但没有编译期类型检查(见 4.5)。</summary>
public sealed class KnownCarrierNumberRows : IEnumerable<object[]>
{
    public IEnumerator<object[]> GetEnumerator()
    {
        // 数据从被测代码的公开承运商表生成:新增承运商时,这里不用改
        foreach (var carrier in TrackingNumberParser.KnownCarriers)
            yield return new object[] { carrier, carrier + "123456789012" };
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public class ClassDataTests
{
    [Theory]
    [ClassData(typeof(InvalidTrackingNumberData))]
    public void TryParse_InvalidTrackingNumber_ReturnsFalse(string? input)
        => Assert.False(TrackingNumberParser.TryParse(input, out _));

    [Theory]
    [ClassData(typeof(InvalidTrackingNumberData))]
    public void Parse_InvalidTrackingNumber_ThrowsFormatExceptionNamingInput(string? input)
    {
        var exception = Assert.Throws<FormatException>(() => TrackingNumberParser.Parse(input));

        Assert.Contains(input ?? string.Empty, exception.Message);
    }

    [Theory]
    [ClassData(typeof(KnownCarrierNumberRows))]
    public void TryParse_EveryKnownCarrier_AcceptsItsNumber(string expectedCarrier, string input)
    {
        Assert.True(TrackingNumberParser.TryParse(input, out var parsed));

        Assert.Equal(expectedCarrier, parsed.Carrier);
    }
}
