namespace Markwardt;

public static class LiteNetUtils
{
    public static DeliveryMethod GetDeliveryMethod(this NetworkReliability mode)
        => mode switch
        {
            NetworkReliability.Unreliable => DeliveryMethod.Unreliable,
            NetworkReliability.Reliable => DeliveryMethod.ReliableUnordered,
            NetworkReliability.Ordered => DeliveryMethod.ReliableOrdered,
            _ => throw new NotSupportedException(mode.ToString())
        };

    public static async ValueTask Run(this NetManager manager, CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            if (manager.IsRunning)
            {
                manager.PollEvents();
            }

            await Task.Delay(25, cancellation);
        }
    }
}