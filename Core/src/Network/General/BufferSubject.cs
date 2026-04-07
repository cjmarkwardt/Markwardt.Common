namespace Markwardt;

public class BufferSubject<T> : BaseDisposable, ISubject<T>
{
    private readonly Queue<Event> queue = [];
    private readonly Subject<T> subject = new();

    public void OnCompleted()
        => Push(new(EventType.Completed, default, default));

    public void OnError(Exception error)
        => Push(new(EventType.Error, default, error));

    public void OnNext(T value)
        => Push(new(EventType.Next, value, default));

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
            queue.Enqueue(push);
        }
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        IDisposable subscription = subject.Subscribe(observer);

        while (queue.TryDequeue(out Event push))
        {
            Push(push);
        }

        return subscription;
    }

    protected override void OnDispose()
    {
        base.OnDispose();

        subject.Dispose();
        queue.ForEach(x => x.TryDispose());
        queue.Clear();
    }

    private enum EventType
    {
        Next,
        Error,
        Completed
    }

    private record struct Event(EventType Type, T? Value, Exception? Error);
}