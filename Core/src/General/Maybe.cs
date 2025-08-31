namespace Markwardt;

public static class MaybeExtensions
{
    public static Maybe<T> Maybe<T>(this T value)
        => new(value);

    public static Maybe<T> WhereNotNull<T>(this Maybe<T?> maybe)
        where T : class
        => maybe.Where(x => x is not null).Select(x => x!);

    public static Maybe<T> WhereValueNotNull<T>(this Maybe<T?> maybe)
        where T : struct
        => maybe.Where(x => x.HasValue).Select(x => x!.Value);

    public static T ValueOr<T>(this IMaybe<T> maybe, T orValue)
        => maybe.HasValue ? maybe.Value : orValue;

    public static T? ValueOrDefault<T>(this IMaybe<T> maybe)
        => maybe.HasValue ? maybe.Value : default;

    public static Maybe<T> NullToMaybe<T>(this T? value)
        where T : class
        => value is not null ? value.Maybe() : default;
        
    public static T? MaybeToNull<T>(this Maybe<T> value)
        where T : class
        => value.ValueOr(null);
}

public interface IMaybe
{
    bool HasValue { get; }
    object? Value { get; }
}

public interface IMaybe<out T>
{
    bool HasValue { get; }
    T Value { get; }
}

public readonly struct Maybe<T> : IMaybe<T>, IDisposable, IAsyncDisposable, IEquatable<IMaybe>, IMaybe
{
    public static bool operator ==(Maybe<T> x, Maybe<T> y)
        => x.Equals(y);

    public static bool operator !=(Maybe<T> x, Maybe<T> y)
        => !x.Equals(y);

    public Maybe()
    {
        value = default!;
        HasValue = false;
    }

    public Maybe(T value)
    {
        this.value = value;
        HasValue = true;
    }

    private readonly T value;

    public readonly bool HasValue { get; }

    public readonly T Value => HasValue ? value : throw new InvalidOperationException("Has no value");

    object? IMaybe.Value => Value;

    public bool TryGetValue([MaybeNullWhen(false)] out T value)
    {
        if (HasValue)
        {
            value = Value;
            return true;
        }
        else
        {
            value = default!;
            return false;
        }
    }

    public Maybe<TSelected> Select<TSelected>(Func<T, TSelected> select)
        => TryGetValue(out T? outValue) ? select(outValue).Maybe() : default;

    public Maybe<TSelected> Select<TSelected>(Func<T, Maybe<TSelected>> select)
        => TryGetValue(out T? outValue) ? select(outValue) : default;

    public Maybe<T> Where(Func<T, bool> where)
        => TryGetValue(out T? outValue) && where(outValue) ? this : default;

    public Maybe<TCasted> Cast<TCasted>()
        => Select(x => (TCasted)(object?)x!);

    public Maybe<TCasted> OfType<TCasted>()
        => Where(x => x is TCasted).Cast<TCasted>();

    public bool TryCast<TCasted>(out Maybe<TCasted> casted)
    {
        if (!HasValue || Value is not TCasted castedValue)
        {
            casted = default;
            return false;
        }
        else
        {
            casted = castedValue.Maybe();
            return true;
        }
    }

    public override string ToString()
        => HasValue ? Value?.ToString() ?? "<NULL>" : "<EMPTY>";

    public void Dispose()
    {
        if (HasValue && Value is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (HasValue && Value is IAsyncDisposable disposable)
        {
            await disposable.DisposeAsync();
        }
    }

    public bool Equals(IMaybe? other)
        => other is not null && ((!HasValue && !other.HasValue) || (HasValue && other.HasValue && Value.ValueEquals(other.Value)));

    public override bool Equals(object? obj)
        => obj is IMaybe maybe && Equals(maybe);

    public override int GetHashCode()
        => HashCode.Combine(HasValue, value);
}