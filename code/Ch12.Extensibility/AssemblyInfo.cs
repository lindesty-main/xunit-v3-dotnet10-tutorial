using Ch12.Extensibility.Production;
using Ch12.MyExtensions;
using Xunit.Sdk;
using Xunit.v3;

// 注册外部序列化器:让 ProductKey 可序列化,数据行才能被逐行运行(Test Explorer / 跨进程 runner)。
[assembly: RegisterXunitSerializer(typeof(ProductKeySerializer), typeof(ProductKey))]

// 注册管道启动:发现与执行都会先跑 StartAsync。
// 4.0 起也可写泛型版 [assembly: TestPipelineStartup<RunStampPipelineStartup>](仅 .NET 8+)。
[assembly: TestPipelineStartup(typeof(RunStampPipelineStartup))]
