namespace Markwardt.Network;

public class Packet : IRecyclable, IPrioritizable, IInspectable
{
    private static readonly Pool<Packet> pool = new(() => new());

    public static Packet New(object? content, IRecyclable? recycler = null)
    {
        Packet packet = pool.Get();
        packet.SetContent(content, recycler);
        return packet;
    }

    private Packet() { }

    private readonly Dictionary<IInspectKey, object> inspections = [];

    public object? Content { get; set; }

    public string? Id { get; set; }
    public int Priority { get; set; }
    public Reliability Reliability { get; set; }
    public IRecyclable? Recycler { get; set; }
    public ISender? Responder { get; set; }
    public object? Source { get; set; }

    IDictionary<IInspectKey, object> IInspectable.Inspections => inspections;

    public void SetContent(object? content, IRecyclable? recycler = null)
    {
        RecycleContent();
        Content = content;
        Recycler = recycler ?? content as IRecyclable;
    }

    public T GetContent<T>()
        => (T)Content!;

    public Maybe<Packet<T>> TryAsContent<T>()
        => Content is T ? new Packet<T>(this).Maybe() : default;

    public Packet Configure(Action<Packet>? configure)
    {
        configure?.Invoke(this);
        return this;
    }

    public void RecycleContent()
    {
        Recycler?.Recycle();
        Content = default;
        Recycler = default;
    }

    public Packet Copy()
    {
        Packet copy = pool.Get();

        copy.Content = Content;
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
}

public readonly record struct Packet<T>(Packet Value)
{
    public readonly bool CanRespond => Value.Responder is not null;
    public readonly T Content => Value.GetContent<T>();

    public void Respond(T content, Action<Packet>? configure = null)
        => Value.Responder?.Send(Packet.New(content).Configure(configure));

    public void Recycle()
        => Value.Recycle();
}