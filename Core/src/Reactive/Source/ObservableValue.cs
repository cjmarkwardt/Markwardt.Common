namespace Markwardt;

public interface IObservableValue
{
    Type ValueType { get; }
    object? Value { get; }
    IObservable<object?> Changes { get; }
}

public interface IObservableValue<T> : IObservable<T>
{
    T Value { get; }
}

public static class ObservableValueExtensions
{
    public static ISourceValue<TSelected> SelectValue<T, TSelected>(this IObservableValue<T> source, Func<T, Maybe<TSelected>> selector, Maybe<TSelected> value = default)
        => new SourceValue<TSelected>(value.HasValue ? value.Value : selector(source.Value).Value, source.Select(selector).Where(x => x.HasValue).Select(x => x.Value));

    public static ISourceValue<TSelected> SelectValue<T, TSelected>(this IObservableValue<T> source, Func<T, TSelected> selector)
        => new SourceValue<TSelected>(selector(source.Value), source.Select(selector));

    public static ISourceValue<T> WhereValue<T>(this IObservableValue<T> source, Func<T, bool> predicate, Maybe<T> value = default)
        => new SourceValue<T>(value.HasValue ? value.Value : source.Value, source.Where(predicate));
}

public abstract class ObservableValue<T> : IObservableValue<T>, IObservableValue
{
    Type IObservableValue.ValueType => typeof(T);
    IObservable<object?> IObservableValue.Changes => this.Select(x => (object?)x);

    public T Value => GetValue();

    object? IObservableValue.Value => Value;

    public abstract IDisposable Subscribe(IObserver<T> observer);

    protected abstract T GetValue();
}