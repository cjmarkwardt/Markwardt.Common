namespace Markwardt;

public interface IMessageChannelValue<T> : IDisposable
{
    T Value { get; set; }

    void Assert(T value);
    void Assert();
}

public class MessageChannelValue<T, TContent> : BaseDisposable, IMessageChannelValue<T>
{
    public MessageChannelValue(IMessageChannel<TContent> channel, TimeSpan sendInterval, T value, Func<T, TContent> write)
    {
        this.channel = channel;
        this.sendInterval = sendInterval;
        this.value = value;
        this.write = write;

        this.RunInBackground(StartSend);
    }

    private readonly IMessageChannel<TContent> channel;
    private readonly TimeSpan sendInterval;
    private readonly Func<T, TContent> write;

    private bool isChanged = true;

    private T value;
    public T Value
    {
        get => value;
        set
        {
            if (!value.ValueEquals(this.value))
            {
                this.value = value;
                isChanged = true;
            }
        }
    }

    public void Assert(T value)
    {
        if (!value.ValueEquals(this.value))
        {
            this.value = value;
            isChanged = false;
            channel.Asserter.Send(write(value));
        }
    }

    public void Assert()
    {
        if (isChanged)
        {
            isChanged = false;
            channel.Asserter.Send(write(value));
        }
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        channel.Dispose();
    }

    private async ValueTask StartSend(CancellationToken cancellation)
    {
        while (!cancellation.IsCancellationRequested)
        {
            if (isChanged)
            {
                isChanged = false;
                channel.Send(write(value));
            }

            await Task.Delay(sendInterval, cancellation);
        }
    }
}