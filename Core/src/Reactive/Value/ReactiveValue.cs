namespace Markwardt;

public interface IReactiveValue<T> : ISourceValue<T>, IReactiveAttachable<T>
{
    void Alter(T value);
}

public static class ReactiveValueExtensions
{
    public static void Set<T>(this IReactiveValue<T> source, T value, IObservable<T> changes)
    {
        bool valueSet = false;
        using IDisposable setSubscription = source.Take(1).Subscribe(x => valueSet = true);

        source.Attach(changes);

        if (!valueSet)
        {
            source.Alter(value);
        }
    }

    public static void Connect<T>(this IReactiveValue<T> source, IObservableValue<T> target)
        => source.Set(target.Value, target);
}

public class ReactiveValue<T> : ObservableValue<T>, IReactiveValue<T>, ISourceValue.IAccessor
{
    public ReactiveValue(T value, IObservable<T>? changes = null)
    {
        this.value = value;

        if (changes is not null)
        {
            Attach(changes);
        }
    }

    private readonly Subject<T> changes = new();
    private readonly WeakSubscriber<T> subscriber = new();

    private T value;
    public new T Value
    {
        get => value;
        set
        {
            Detach();
            Set(value);
        }
    }

    public new ISourceValue.IAccessor Accessor => throw new NotImplementedException();

    public void Alter(T value)
        => Set(value);

    public void Attach(IObservable<T> changes)
        => subscriber.Subscribe(changes, Set);

    public void Detach()
        => subscriber.Unsubscribe();

    public override IDisposable Subscribe(IObserver<T> observer)
        => changes.Subscribe(observer);

    protected override T GetValue()
        => Value;

    private void Set(T value)
    {
        if (!this.value.ValueEquals(value))
        {
            this.value = value;
            changes.OnNext(value);
        }
    }

    void ISourceValue.IAccessor.Set(object? value)
    {
        throw new NotImplementedException();
    }

    /*public SourceValue(T value, IObservable<T>? changes = null)
    {
        this.value = value;
        Subscribe(changes);
    }

    private readonly Subject<T> changes = new();
    private readonly WeakSubscriber<T> subscriber = new();

    private T value;

    public override T Get()
        => value;

    public void Set(T value, bool unsubscribe = true)
    {
        if (!this.value.ValueEquals(value))
        {
            this.value = value;
            changes.OnNext(value);
        }

        if (unsubscribe)
        {
            subscriber.Unsubscribe();
        }
    }

    public void Observe(IObservable<T>? changes)
    {
        throw new NotImplementedException();
    }

    public void Connect(IObservable<T> changes, bool tryInitialize = true)
    {
        if (changes is IObservableValue<T> value)
        {
            Set(value.Get());
        }

        Subscribe(changes);
    }

    public void Connect(T value, IObservable<T>? changes = null)
    {
        Set(value);
        Subscribe(changes);
    }

    private void Subscribe(IObservable<T>? source = null)
    {
        if (source is not null)
        {
            subscriber.Subscribe(source, Set);
        }
        else
        {
            subscriber.Unsubscribe();
        }
    }*/
}