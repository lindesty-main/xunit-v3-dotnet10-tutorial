using System.Globalization;
using System.Reflection;
using Xunit.v3;
using Xunit.Sdk;

namespace Ch12.MyExtensions;

/// <summary>
/// 最小自定义数据源:把内嵌 CSV(每行一条记录,逗号分隔)解析成理论数据行。
/// 行首的 ! 标记该行跳过(行级 Skip 元数据);特性上的 Label/Skip/Traits 等
/// 命名参数要经基类的 ConvertDataRow 才会套到每条数据行上(v3 没有发现器,
/// 特性对自身行为负责),例如 [CsvData(Csv, Label = "inventory")]。
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class CsvDataAttribute(string csv) : DataAttribute
{
    public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(
        MethodInfo testMethod,
        DisposalTracker disposalTracker)
    {
        var parameterTypes = testMethod.GetParameters().Select(p => p.ParameterType).ToArray();
        var rows = new List<ITheoryDataRow>();

        foreach (var rawLine in csv.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var line = rawLine;
            var markSkipped = line.StartsWith('!');
            if (markSkipped)
                line = line[1..].Trim();

            var cells = line.Split(',', StringSplitOptions.TrimEntries);
            if (cells.Length != parameterTypes.Length)
                throw new ArgumentException(
                    $"CSV 行 \"{line}\" 有 {cells.Length} 列,而测试方法 {testMethod.Name} 有 {parameterTypes.Length} 个参数");

            var values = new object?[cells.Length];
            for (var i = 0; i < cells.Length; i++)
            {
                var target = Nullable.GetUnderlyingType(parameterTypes[i]) ?? parameterTypes[i];
                values[i] = Convert.ChangeType(cells[i], target, CultureInfo.InvariantCulture);
            }

            TheoryDataRowBase row = new TheoryDataRow(values);
            if (markSkipped)
                row = row.WithSkip("CSV 行首带 !,标记为暂不执行");
            // ConvertDataRow:行上已有的值优先,缺的从特性属性(Label/Skip/Traits/…)补齐
            rows.Add(ConvertDataRow(row));
        }

        return ValueTask.FromResult<IReadOnlyCollection<ITheoryDataRow>>(rows);
    }

    /// <summary>
    /// true = 发现期就枚举数据,每行成为独立测试用例(Test Explorer / 开预枚举的
    /// runner 才能逐行选中、逐行统计);false = 只产出一个测试用例,执行期才枚举
    /// 数据 —— 数据源要连库/读文件时,可避免发现期就去读它的开销。
    /// </summary>
    public override bool SupportsDiscoveryEnumeration() => true;
}
