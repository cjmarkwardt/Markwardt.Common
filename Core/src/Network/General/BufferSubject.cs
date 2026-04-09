namespace Markwardt;

public class BufferSubject<T> : BaseDisposable, ISubject<T>
{
    private readonly LinkedList<Event> queue = [];
    private readonly ExtendedDictionary<T, Queue<LinkedListNode<Event>>> nodeLookup = [];
    private readonly Subject<T> subject = new();

    public void OnCompleted()
        => Push(new(EventType.Completed, default, default));

    public void OnError(Exception error)
        => Push(new(EventType.Error, default, error));

    public void OnNext(T value)
        => Push(new(EventType.Next, value, default));

    public void Remove(T value)
    {
        if (nodeLookup.TryGetValue(value, out Queue<LinkedListNode<Event>>? nodes) && nodes.TryDequeue(out LinkedListNode<Event>? node))
        {
            queue.Remove(node);

            if (nodes.Count == 0)
            {
                nodeLookup.Remove(value);
            }
        }
    }

    private void Push(Event push)
    {
        if (subject.HasObservers)
        {
            switch (push.Type)
            {
                case EventType.Next:
                    subject.OnNext(push.Value!);
                    break;
                case EventType.Error:
                    subject.OnError(push.Error!);
                    break;
                case EventType.Completed:
                    subject.OnCompleted();
                    break;
            }
        }
        else
        {
            LinkedListNode<Event> node = queue.AddLast(push);

            if (push.Type is EventType.Next)
            {
                if (!nodeLookup.TryGetValue(push.Value!, out Queue<LinkedListNode<Event>>? nodes))
                {
                    nodes = new();
                    nodeLookup.Add(push.Value!, nodes);
                }

                nodes.Enqueue(node);
            }
        }
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        IDisposable subscription = subject.Subscribe(observer);

        nodeLookup.Clear();
        while (queue.First is not null)
        {
            Event node = queue.First.Value;
            queue.RemoveFirst();
            Push(node);
        }

        return subscription;
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        subject.Dispose();
        queue.ForEach(x => x.TryDispose());
        queue.Clear();
        nodeLookup.Clear();
    }

    private enum EventType
    {
        Next,
        Error,
        Completed
    }

    private record struct Event(EventType Type, T? Value, Exception? Error);
}