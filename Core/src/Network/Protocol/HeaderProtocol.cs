namespace Markwardt;

public abstract class HeaderProcessor<T, THeader> : MessageProcessor<T>
    where THeader : class
{
    protected HeaderProcessor()
    {
        interceptor = CreateInterceptor();
    }

    private readonly IMessageInterceptor? interceptor;

    protected override IEnumerable<IMessageInterceptor> Interceptors => base.Interceptors.Concat(interceptor is not null ? [interceptor] : []);

    protected abstract InspectKey<THeader> HeaderKey { get; }

    protected abstract void SetHeader(T content, THeader header);
    protected abstract Maybe<THeader> GetHeader(T content);

    protected virtual IMessageInterceptor? CreateInterceptor()
        => null;

    protected override void SendContent(Message message, T content)
    {
        if (message.Inspect(HeaderKey).TryGetValue(out THeader? header))
        {
            SetHeader(content, header);
        }

        TriggerSent(message);
    }

    protected override void ReceiveContent(Message message, T content)
    {
        if (GetHeader(content).TryGetValue(out THeader? header))
        {
            message.SetInspect(HeaderKey, header);
        }

        TriggerReceived(message);
    }

    protected override void OnDisconnected(Exception? exception)
    {
        base.OnDisconnected(exception);

        interceptor?.TryDispose();
    }
}

public abstract class ValueHeaderProcessor<T, THeader> : HeaderProcessor<T, ValueWrapper<THeader>>
    where THeader : struct
{
    protected abstract InspectValueKey<THeader> ValueHeaderKey { get; }

    protected sealed override InspectKey<ValueWrapper<THeader>> HeaderKey => ValueHeaderKey;

    protected abstract void SetValueHeader(T content, THeader header);
    protected abstract Maybe<THeader> GetValueHeader(T content);

    protected sealed override void SetHeader(T content, ValueWrapper<THeader> header)
        => SetValueHeader(content, header.Value);

    protected sealed override Maybe<ValueWrapper<THeader>> GetHeader(T content)
        => GetValueHeader(content).TryGetValue(out THeader header) ? new ValueWrapper<THeader>() { Value = header }.Maybe() : default;
}