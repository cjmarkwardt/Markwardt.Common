namespace Markwardt.Network;

public static class PacketExtensions
{
    public static Packet<ReadOnlyMemory<byte>> SetContent(this Packet<ReadOnlyMemory<byte>> packet, Buffer<byte> buffer)
        => packet.SetContent(buffer.Memory.AsReadOnly(), buffer);
}

public class Packet : IRecyclable, IPrioritizable, IInspectable
{
    private static readonly Pool<Packet> pool = new(() => new());

    public static Packet New(object? value, IRecyclable? recycler = null)
    {
        Packet packet = pool.Get();
        packet.Set(value, recycler);
        return packet;
    }

    public static Packet<ReadOnlyMemory<T>> FromBuffer<T>(Buffer<T> buffer, int? length = null)
        => New(buffer.Memory[..(length ?? buffer.Length)].AsReadOnly(), buffer).As<ReadOnlyMemory<T>>();

    private Packet() { }

    private readonly Dictionary<IInspectKey, object> inspections = [];

    private Maybe<object?> value = default;
    public object? Value { get => value.Value; set => this.value = value.Maybe(); }

    public string? Id { get; set; }
    public int Priority { get; set; }
    public Reliability Reliability { get; set; }
    public IRecyclable? Recycler { get; set; }
    public ISender? Responder { get; set; }
    public object? Source { get; set; }

    IDictionary<IInspectKey, object> IInspectable.Inspections => inspections;

    public void Set(object? value, IRecyclable? recycler = null)
    {
        RecycleContent();
        this.value = value.Maybe();
        Recycler = recycler ?? value as IRecyclable;
    }

    public Packet<T> As<T>()
        => new(this);

    public Packet Configure(Action<Packet>? configure)
    {
        configure?.Invoke(this);
        return this;
    }

    public Packet Copy()
    {
        Packet copy = pool.Get();

        copy.value = value;
        copy.Id = Id;
        copy.Priority = Priority;
        copy.Reliability = Reliability;
        copy.Recycler = Recycler;
        copy.Responder = Responder;
        copy.Source = Source;
        copy.CopyInspects(this);
        
        return copy;
    }

    public void Recycle()
    {
        RecycleContent();

        Id = default;
        Priority = default;
        Reliability = default;
        Responder = default;
        Source = default;
        inspections.Clear();

        pool.Recycle(this);
    }

    public void RecycleContent()
    {
        Recycler?.Recycle();
        value = default;
        Recycler = default;
    }
}

public readonly record struct Packet<T>(Packet Inner)
{
    public static implicit operator Packet(Packet<T> packet)
        => packet.Inner;

    public static Packet<T> New(T content, IRecyclable? recycler = null)
        => Packet.New(content, recycler).As<T>();

    public static Packet<T> NewSignal(object signal, IRecyclable? recycler = null)
        => Packet.New(signal, recycler).As<T>();

    public readonly bool CanRespond => Inner.Responder is not null;

    public readonly bool IsContent => Inner.Value is T;
    public readonly bool IsSignal => Inner.Value is not T && Inner.Value is not null;

    public readonly T Content => IsContent ? (T)Inner.Value! : throw new InvalidOperationException("Packet does not contain content of type " + typeof(T).Name);
    public readonly object Signal => IsSignal ? Inner.Value! : throw new InvalidOperationException("Packet does not contain a signal");

    public readonly string? Id { get => Inner.Id; set => Inner.Id = value; }
    public readonly int Priority { get => Inner.Priority; set => Inner.Priority = value; }
    public readonly Reliability Reliability { get => Inner.Reliability; set => Inner.Reliability = value; }
    public readonly IRecyclable? Recycler { get => Inner.Recycler; set => Inner.Recycler = value; }
    public readonly ISender? Responder { get => Inner.Responder; set => Inner.Responder = value; }
    public readonly object? Source { get => Inner.Source; set => Inner.Source = value; }

    public Packet<T2> As<T2>()
        => new(Inner);

    public Packet<T> SetContent(T value, IRecyclable? recycler = null)
    {
        Inner.Set(value, recycler);
        return this;
    }

    public Packet<T> SetSignal(object signal)
    {
        Inner.Set(signal);
        return this;
    }

    public void Respond(T content, Action<Packet>? configure = null)
        => Inner.Responder.NotNull("Cannot respond when responder is null").Send(Packet.New(content).Configure(configure));

    public void Recycle()
        => Inner.Recycle();
}