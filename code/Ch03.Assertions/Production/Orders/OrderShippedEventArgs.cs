namespace Ch03.Assertions.Production.Orders;

public sealed class OrderShippedEventArgs(string trackingNumber) : EventArgs
{
    public string TrackingNumber { get; } = trackingNumber;
}
