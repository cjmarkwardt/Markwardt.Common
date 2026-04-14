namespace Markwardt.Network;

public static class PacketExtensions
{
    public static Packet<ReadOnlyMemory<byte>> SetContent(this Packet<ReadOnlyMemory<byte>> packet, Buffer<byte> buffer)
        => packet.SetContent(buffer.Memory.AsReadOnly(), buffer);
}

public class Packet : IRecyclable, IPrioritizable, IInspectable
{
    private static readonly Pool<Packet> pool = new(() => new());

    public static Packet<T> New<T>(T value, IRecyclable? recycler = null)
        => pool.Get().As<T>().SetContent(value, recycler);

    public static Packet<T> NewSignal<T>(object signal, IRecyclable? recycler = null)
        => pool.Get().As<T>().SetSignal(signal, recycler);

    public static Packet<ReadOnlyMemory<T>> FromBuffer<T>(Buffer<T> buffer, int? length = null)
        => New(buffer.Memory[..(length ?? buffer.Length)].AsReadOnly(), buffer).As<ReadOnlyMemory<T>>();

    private Packet() { }

    private readonly Dictionary<IInspectKey, object> inspections = [];

    public bool IsSignal => !IsContent;

    public bool IsContent { get; private set; }
    public object? Value { get; private set; }

    public string? Id { get; set; }
    public int Priority { get; set; }
    public Reliability Reliability { get; set; }
    public IRecyclable? Recycler { get; set; }
    public Action<Packet>? Responder { get; set; }
    public object? Source { get; set; }

    IDictionary<IInspectKey, object> IInspectable.Inspections => inspections;

    public void SetContent(object? content, IRecyclable? recycler = null, bool recycle = true)
    {
        if (recycle)
        {
            RecycleContent();
        }

        Value = content;
        IsContent = true;
        Recycler = recycler ?? content as IRecyclable;
    }

    public void SetSignal(object signal, IRecyclable? recycler = null, bool recycle = true)
    {
        if (recycle)
        {
            RecycleContent();
        }

        Value = signal;
        IsContent = false;
        Recycler = recycler ?? signal as IRecyclable;
    }

    public Packet<T> As<T>()
        => new(this);

    public Packet<T2> AsContent<T2>(T2 content, IRecyclable? recycler = null, bool recycle = true)
        => As<T2>().SetContent(content, recycler, recycle);

    public Packet<T2> AsSignal<T2>(object signal, IRecyclable? recycler = null, bool recycle = true)
        => As<T2>().SetSignal(signal, recycler, recycle);

    public Packet Configure(Action<Packet>? configure)
    {
        configure?.Invoke(this);
        return this;
    }

    public Packet Copy()
    {
        Packet copy = pool.Get();

        copy.Value = Value;
        copy.IsContent = IsContent;
        copy.Id = Id;
        copy.Priority = Priority;
        copy.Reliability = Reliability;
        copy.Recycler = Recycler;
        copy.Responder = Responder;
        copy.Source = Source;
        copy.CopyInspects(this);
        
        return copy;
    }

    public Packet<T> Copy<T>()
        => Copy().As<T>();

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

    private void RecycleContent()
    {
        Recycler?.Recycle();
        Value = default;
        Recycler = default;
    }
}

public readonly record struct Packet<T>(Packet Inner)
{
    public static implicit operator Packet(Packet<T> packet)
        => packet.Inner;

    public readonly bool CanRespond => Inner.Responder is not null;

    public readonly bool IsContent => Inner.IsContent;
    public readonly bool IsSignal => Inner.IsSignal;

    public readonly T Content => IsContent ? (T)Inner.Value! : throw new InvalidOperationException("Packet does not contain content of type " + typeof(T).Name);
    public readonly object Signal => IsSignal ? Inner.Value! : throw new InvalidOperationException("Packet does not contain a signal");

    public readonly string? Id { get => Inner.Id; set => Inner.Id = value; }
    public readonly int Priority { get => Inner.Priority; set => Inner.Priority = value; }
    public readonly Reliability Reliability { get => Inner.Reliability; set => Inner.Reliability = value; }
    public readonly IRecyclable? Recycler { get => Inner.Recycler; set => Inner.Recycler = value; }
    public readonly Action<Packet>? Responder { get => Inner.Responder; set => Inner.Responder = value; }
    public readonly object? Source { get => Inner.Source; set => Inner.Source = value; }

    public Packet<T> Configure(Action<Packet<T>>? configure)
    {
        configure?.Invoke(this);
        return this;
    }

    public Packet<T> SetContent(T value, IRecyclable? recycler = null, bool recycle = true)
    {
        Inner.SetContent(value, recycler, recycle);
        return this;
    }

    public Packet<T> SetSignal(object signal, IRecyclable? recycler = null, bool recycle = true)
    {
        Inner.SetSignal(signal, recycler, recycle);
        return this;
    }
    
    public Packet<T> Copy()
        => Inner.Copy().As<T>();

    public Packet<T2> Copy<T2>()
        => Inner.Copy<T2>();

    public Packet<T2> As<T2>()
        => Inner.As<T2>();

    public Packet<T2> AsContent<T2>(T2 content, IRecyclable? recycler = null, bool recycle = true)
        => Inner.AsContent(content, recycler, recycle);

    public Packet<T2> AsSignal<T2>(object signal, IRecyclable? recycler = null, bool recycle = true)
        => Inner.AsSignal<T2>(signal, recycler, recycle);

    public void Respond(Packet<T> packet)
        => Inner.Responder.NotNull("Cannot respond when responder is null").Invoke(packet);

    public void Respond(T content, Action<Packet<T>>? configure = null)
        => Respond(Packet.New(content).Configure(configure));

    public void Recycle()
        => Inner.Recycle();
}