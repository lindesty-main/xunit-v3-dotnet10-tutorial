using System.Globalization;

namespace Ch10.Reporting.Production;

public class PriceTagPrinter
{
    // 依赖当前线程的 culture:同一个输入在不同 culture 下输出不同
    public string FormatPrice(double amount) =>
        amount.ToString("N2");

    public DateTime ParseValidUntil(string text) =>
        DateTime.Parse(text, CultureInfo.CurrentCulture);

    // 正确做法:显式传入固定 culture,结果与运行环境无关
    public string FormatPriceInvariant(double amount) =>
        amount.ToString("N2", CultureInfo.InvariantCulture);

    // 没有任何测试调用它:覆盖率报告里它的 line-rate 会是 0(见 10.5)
    public string FormatPricePlain(double amount) =>
        amount.ToString("N");
}
