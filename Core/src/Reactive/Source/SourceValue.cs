namespace Markwardt;

public interface ISourceValue : IObservableValue
{
    new object? Value { get;set; }
}

public interface ISourceValue<T> : IObservableValue<T>, ISourceAttachable<T>
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

    public static ISourceValue<T> ToSourceValue<T>(this IObservable<T> source, T value)
        => new SourceValue<T>(value, source);
}

public class SourceValue<T> : ObservableValue<T>, ISourceValue<T>, ISourceValue
{
    public SourceValue()
    {
        isSet = false;
        value = default!;
    }

    public SourceValue(T value, IObservable<T>? changes = null)
    {
        isSet = true;
        this.value = value;

        if (changes is not null)
        {
            Attach(changes);
        }
    }

    private readonly Subject<T> changes = new();
    private readonly WeakSubscriber<T> subscriber = new();

    private bool isSet;
    private T value;
    public new T Value
    {
        get => isSet ? value : throw new InvalidOperationException("Value has not been set.");
        set
        {
            Detach();
            Set(value);
        }
    }

    object? ISourceValue.Value { get => Value; set => Value = (T)value!; }

    public void Alter(T value)
        => Set(value);

    public void Attach(IObservable<T> changes)
        => subscriber.Subscribe(changes, Set);

    public void Detach()
        => subscriber.Unsubscribe();

    public override IDisposable Subscribe(IObserver<T> observer)
        => changes.Subscribe(observer);

    public override string ToString()
        => value?.ToString() ?? string.Empty;

    protected override T GetValue()
        => Value;

    private void Set(T value)
    {
        isSet = true;
        
        if (!this.value.ValueEquals(value))
        {
            this.value = value;
            changes.OnNext(value);
        }
    }
}