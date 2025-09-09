namespace Markwardt;

public interface IObservableValue
{
    IAccessor Accessor { get; }

    interface IAccessor
    {
        Type ValueType { get; }
        object? Value { get; }
        IObservable<object?> Changes { get; }
    }
}

public interface IObservableValue<T> : IObservableValue, IObservable<T>
{
    T Value { get; }
}

public abstract class ObservableValue<T> : IObservableValue<T>, IObservableValue.IAccessor
{
    public IObservableValue.IAccessor Accessor => this;

    Type IObservableValue.IAccessor.ValueType => typeof(T);
    IObservable<object?> IObservableValue.IAccessor.Changes => this.Select(x => (object?)x);

    public T Value => GetValue();

    object? IObservableValue.IAccessor.Value => Value;

    public abstract IDisposable Subscribe(IObserver<T> observer);

    protected abstract T GetValue();
}