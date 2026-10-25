# xUnit.net 实战教程

面向 .NET 开发者的 xUnit.net 系统性教程:每一章一篇可独立阅读的文档,配套一个或多个可直接运行的示例工程。个别章节还包含供示例使用的扩展库。

- 运行时:**.NET 10**(`net10.0`)
- 测试框架:**xUnit.net v3(4.0.2)**,默认集成 Microsoft Testing Platform(MTP)v2
- 验证环境:.NET SDK 10.0.113,所有示例代码均已编译并通过测试运行

## 环境要求

1. 安装 [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)(`dotnet --version` 应输出 `10.0.x`)。
2. 安装 xUnit.net v3 项目模板(仅首次):

   ```bash
   dotnet new install xunit.v3.templates
   ```

## 如何运行示例代码

全部示例位于 `code/`,是一个完整解决方案([`code/XUnitGuide.slnx`](code/XUnitGuide.slnx)),每个章工程相互独立:

```bash
cd code

dotnet test

# 只运行某一章
dotnet test --project Ch03.Assertions

# 直接运行章工程的可执行文件(xUnit 原生命令行)
dotnet run --project Ch03.Assertions -- -?
```

[`code/global.json`](code/global.json) 已把 `dotnet test` 配置为 MTP 模式(.NET 10 SDK 下的推荐方式),无需额外参数。

## 目录

| 章 | 文档 | 示例工程 |
| --- | --- | --- |
| 01 | [认识 xUnit.net 与第一个测试](docs/01-认识xunit与第一个测试.md) | [`code/Ch01.GettingStarted`](code/Ch01.GettingStarted) |
| 02 | [测试的结构与命名](docs/02-测试结构与命名.md) | [`code/Ch02.TestStructure`](code/Ch02.TestStructure) |
| 03 | [断言](docs/03-断言.md) | [`code/Ch03.Assertions`](code/Ch03.Assertions) |
| 04 | [参数化测试](docs/04-参数化测试.md) | [`code/Ch04.Parameterized`](code/Ch04.Parameterized) |
| 05 | [夹具与生命周期](docs/05-夹具与生命周期.md) | [`code/Ch05.Lifecycle`](code/Ch05.Lifecycle) |
| 06 | [并行执行与执行顺序](docs/06-并行执行与顺序.md) | [`code/Ch06.Parallel`](code/Ch06.Parallel) |
| 07 | [条件执行与测试筛选](docs/07-条件执行与筛选.md) | [`code/Ch07.Conditional`](code/Ch07.Conditional) |
| 08 | [超时与取消](docs/08-超时与取消.md) | [`code/Ch08.Timeouts`](code/Ch08.Timeouts) |
| 09 | [输出、诊断与调试](docs/09-输出诊断与调试.md) | [`code/Ch09.Output`](code/Ch09.Output)、[`code/Ch09.Output.Capture`](code/Ch09.Output.Capture) |
| 10 | [配置、报告与代码覆盖率](docs/10-配置报告与覆盖率.md) | [`code/Ch10.Reporting`](code/Ch10.Reporting) |
| 11 | [持续集成](docs/11-持续集成.md) | [`code/Ch11.Ci`](code/Ch11.Ci) |
| 12 | [扩展与复用](docs/12-扩展与复用.md) | [`code/Ch12.Extensibility`](code/Ch12.Extensibility)、扩展库 [`code/Ch12.MyExtensions`](code/Ch12.MyExtensions) |
| 13 | [结语与进阶路线](docs/13-结语与进阶路线.md) | — |

## 学习建议

按章节顺序阅读:第 1–2 章建立正确的测试观,第 3–5 章是日常编写测试的核心技能,第 6–12 章逐步进入执行控制、工程化与框架扩展。每章末尾附练习与验收标准。
