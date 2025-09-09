namespace Markwardt;

public interface ISourceValue : IObservableValue
{
    new IAccessor Accessor { get; }

    new interface IAccessor : IObservableValue.IAccessor
    {
        void Set(object? value);
    }
}

public interface ISourceValue<T> : ISourceValue, IObservableValue<T>, ISourceAttachable<T>
{
    new T Value { get; set; }

    void Alter(T value);
}

public static class SourceValueExtensions
{
    public static void Set<T>(this ISourceValue<T> source, T value, IObservable<T> changes)
    {
        bool valueSet = false;
        using IDisposable setSubscription = source.Take(1).Subscribe(x => valueSet = true);

        source.Attach(changes);

        if (!valueSet)
        {
            source.Alter(value);
        }
    }

    public static void Connect<T>(this ISourceValue<T> source, IObservableValue<T> target)
        => source.Set(target.Value, target);
}

public class SourceValue<T> : ObservableValue<T>, ISourceValue<T>, ISourceValue.IAccessor
{
    public SourceValue(T value, IObservable<T>? changes = null)
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
        => Set((T)value!);
}