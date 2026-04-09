namespace Markwardt;

public class Message : IRecyclable, IPrioritizable, IInspectable
{
    private static readonly Pool<Message> pool = new(() => new());

    public static Message New(object? content, IRecyclable? recycler = null)
    {
        Message message = pool.Get();
        message.SetContent(content, recycler);
        return message;
    }

    private Message() { }

    private readonly Dictionary<IInspectKey, object> inspections = [];

    public object? Content { get; set; }

    public string? Id { get; set; }
    public int Priority { get; set; }
    public Reliability Reliability { get; set; }
    public IRecyclable? Recycler { get; set; }
    public IMessageSender? Responder { get; set; }
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

    public IMessageSender<T>? GetResponder<T>()
        => Responder is null ? null : new MessageSender<T>(Responder);

    public Message Configure(Action<Message>? configure)
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

    public Message Copy()
    {
        Message copy = pool.Get();

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