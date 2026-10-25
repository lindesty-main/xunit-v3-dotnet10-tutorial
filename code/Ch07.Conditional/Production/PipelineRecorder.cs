using System.Collections.Concurrent;

namespace Ch07.Conditional.Production;

/// <summary>
/// 执行痕迹记录器:按"测试方法名 + 阶段"计数,用来证明某条测试到底跑到了流水线的哪一步。
/// 用 ConcurrentDictionary 是必要的:测试集合之间默认并行。
/// </summary>
public static class PipelineRecorder
{
    public static readonly string[] Stages = ["类构造", "Before", "测试体", "After"];

    private static readonly ConcurrentDictionary<string, int> Counts = new();

    public static void Record(string testName, string stage) =>
        Counts.AddOrUpdate(Key(testName, stage), 1, (_, count) => count + 1);

    public static int Count(string testName, string stage) =>
        Counts.TryGetValue(Key(testName, stage), out var count) ? count : 0;

    /// <summary>把若干测试的痕迹排成文本表,供测试输出与文档引用。</summary>
    public static string Describe(IEnumerable<string> testNames)
    {
        var lines = new List<string> { "测试方法".PadRight(28) + string.Concat(Stages.Select(s => s.PadLeft(10))) };
        foreach (var name in testNames)
            lines.Add(name.PadRight(28) +
                string.Concat(Stages.Select(s => Count(name, s).ToString().PadLeft(10))));
        return string.Join(Environment.NewLine, lines);
    }

    private static string Key(string testName, string stage) => $"{testName}::{stage}";
}
