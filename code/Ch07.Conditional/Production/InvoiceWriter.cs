namespace Ch07.Conditional.Production;

/// <summary>
/// 被测代码:把发票写出到 CH07_EXPORT_DIR 指定的目录。
/// 未配置目录时抛 ExportUnavailableException —— 这是 integration 层测试的前提条件,
/// 也是 7.2.4 里被 SkipExceptions 接住的那个异常。
/// </summary>
public static class InvoiceWriter
{
    public static string Write(string directory, string fileName, string csv)
    {
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, csv);
        return path;
    }

    public static string WriteToConfiguredDirectory(string fileName, string csv)
    {
        var directory = Environment.GetEnvironmentVariable("CH07_EXPORT_DIR")
            ?? throw new ExportUnavailableException("未配置 CH07_EXPORT_DIR,发票无处可写");
        return Write(directory, fileName, csv);
    }
}

/// <summary>外部写出目录未配置时抛出;测试用 SkipExceptions 把它变成跳过。</summary>
public sealed class ExportUnavailableException(string message) : Exception(message);
