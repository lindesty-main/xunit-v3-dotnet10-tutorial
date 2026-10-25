namespace Ch07.Conditional.Production;

/// <summary>被测代码:进程探针。/proc 只在 Linux 上存在,这正是那条用例的前提。</summary>
public static class ProcessProbe
{
    public static string ReadSelfStatus() => File.ReadAllText("/proc/self/status");
}
