namespace Markwardt;

public static class LiteNetUtils
{
    public static DeliveryMethod GetDeliveryMethod(NetworkConstraints constraints)
    {
        if (!constraints.HasFlag(NetworkConstraints.Ordered))
        {
            if (!constraints.HasFlag(NetworkConstraints.Reliable) && !constraints.HasFlag(NetworkConstraints.Distinct))
            {
                return DeliveryMethod.Unreliable;
            }
            else
            {
                return DeliveryMethod.ReliableUnordered;
            }
        }

        return DeliveryMethod.ReliableOrdered;
    }

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