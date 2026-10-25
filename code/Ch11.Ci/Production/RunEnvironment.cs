namespace Ch11.Ci.Production;

/// <summary>
/// 运行环境判定(第 7 章的模式):所有"这次运行在什么环境"的判断集中在这里,
/// 测试与门禁只读结论,不各自散落 Environment.GetEnvironmentVariable。
/// </summary>
public static class RunEnvironment
{
    /// <summary>
    /// CI 判定。GitHub Actions、Azure Pipelines、GitLab CI、Jenkins 等都会设置 CI 环境变量,
    /// 值各不相同(true / 1 / ...),所以只判断"存在且非空"。
    /// </summary>
    public static bool IsCi => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CI"));

    /// <summary>
    /// 集成层使用的产物存储目录。未配置返回 null;
    /// CI 的集成 job 里由工作流设置(GitHub Actions 用 ${{ runner.temp }}/...),
    /// 本地想用同一批集成测试时手动 export 同一个变量即可。
    /// </summary>
    public static string? StoreDirectory =>
        Environment.GetEnvironmentVariable("CH11_STORE_DIR") is { Length: > 0 } value ? value : null;
}
