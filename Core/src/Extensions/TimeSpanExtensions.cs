namespace Markwardt;

public static class TimeSpanExtensions
{
    public static async ValueTask Delay(this TimeSpan? time, CancellationToken cancellation = default)
    {
        if (time is not null)
        {
            await time.Value.Delay(cancellation);
        }
    }

    public static async ValueTask Delay(this TimeSpan time, CancellationToken cancellation = default)
        => await Task.Delay(time, cancellation);
}