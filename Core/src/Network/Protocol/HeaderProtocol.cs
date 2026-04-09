namespace Markwardt.Network;

public interface IHeaderPacket<THeader>
{
    Maybe<THeader> GetHeader();
    void SetHeader(THeader header);
}

public abstract class HeaderrProcessor<T, THeader> : ConnectionProcessor<T>
    where T : IHeaderPacket<THeader>
    where THeader : struct
{
    private readonly InspectValueKey<THeader> headerKey = new(typeof(THeader).Name);

    protected Maybe<THeader> GetHeader(Packet packet)
        => packet.Inspect(headerKey);

    protected void SetHeader(Packet packet, THeader header)
        => packet.SetInspect(headerKey, header);

    protected override void SendContent(Packet packet, T content)
    {
        if (packet.Inspect(headerKey).TryGetValue(out THeader header))
        {
            content.SetHeader(header);
        }

        TriggerSent(packet);
    }

    protected override void ReceiveContent(Packet packet, T content)
    {
        if (content.GetHeader().TryGetValue(out THeader header))
        {
            packet.SetInspect(headerKey, header);
        }

        TriggerReceived(packet);
    }
}

public abstract class HeaderProcessor<T, THeader> : ConnectionProcessor<T>
    where THeader : class
{
    protected HeaderProcessor()
    {
        interceptor = CreateInterceptor();
    }

    private readonly INetworkInterceptor? interceptor;

    protected override IEnumerable<INetworkInterceptor> Interceptors => base.Interceptors.Concat(interceptor is not null ? [interceptor] : []);

    protected abstract InspectKey<THeader> HeaderKey { get; }

    protected abstract void SetHeader(T content, THeader header);
    protected abstract Maybe<THeader> GetHeader(T content);

    protected virtual INetworkInterceptor? CreateInterceptor()
        => null;

    protected override void SendContent(Packet packet, T content)
    {
        if (packet.Inspect(HeaderKey).TryGetValue(out THeader? header))
        {
            SetHeader(content, header);
        }

        TriggerSent(packet);
    }

    protected override void ReceiveContent(Packet packet, T content)
    {
        if (GetHeader(content).TryGetValue(out THeader? header))
        {
            packet.SetInspect(HeaderKey, header);
        }

        TriggerReceived(packet);
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