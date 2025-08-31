namespace Markwardt;

public static class TimeSpanExtensions
{
    public static async ValueTask<Failable> Delay(this TimeSpan? time, CancellationToken cancellation = default)
    {
        try
        {
            if (time is not null)
            {
                await Task.Delay(time.Value, cancellation);
            }

            return Failable.Success();
        }
        catch (OperationCanceledException exception)
        {
            return exception;
        }
    }

    public static async ValueTask<Failable> Delay(this TimeSpan time, CancellationToken cancellation = default)
        => await ((TimeSpan?)time).Delay(cancellation);
}