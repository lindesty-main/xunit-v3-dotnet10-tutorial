namespace Ch04.Parameterized.Tests.TestData;

/// <summary>
/// 跨测试类共享的数据源:遗留系统里已经开出过的运单号,必须永远能被解析。
/// 数据源可以是 async 方法(Task / ValueTask),这样它就能真的去读文件或查库。
/// </summary>
public static class LegacyTrackingNumberRows
{
    private const string legacyNumbers = """
        SF000000000001
        YT100000000002
        JD200000000003
        """;

    public static async Task<IEnumerable<object?[]>> RoundTripRowsAsync()
    {
        // 真实项目里这里通常是 await File.ReadAllLinesAsync(...) 或一次数据库查询
        var lines = legacyNumbers.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var rows = lines.Select(line => new object?[] { line }).ToList();

        await Task.Yield();
        return rows;
    }
}
