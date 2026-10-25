using System.Collections.Concurrent;

namespace Ch05.Lifecycle.Production;

/// <summary>
/// 订单台账:创建与履约。第 5.4 节里它被装进集合夹具,
/// 供多个测试类共用——实现必须是线程安全的。
/// </summary>
public sealed class OrderLedger
{
    public enum OrderState
    {
        Created,
        Fulfilled,
    }

    private readonly ConcurrentDictionary<string, OrderState> states = new();

    public void Create(string orderId) => states[orderId] = OrderState.Created;

    public bool Fulfill(string orderId) =>
        states.TryGetValue(orderId, out var current) &&
        states.TryUpdate(orderId, OrderState.Fulfilled, current);

    public OrderState? StateOf(string orderId) =>
        states.TryGetValue(orderId, out var state) ? state : null;
}
