namespace Markwardt;

public record struct ItemChange<T>(ItemChangeKind Kind, T Item)
{
    public ItemChange<TConverted> Convert<TConverted>(Func<T, TConverted> convert)
        => new(Kind, convert(Item));

    public ItemChange<TCasted> Cast<TCasted>()
        => Convert(x => (TCasted)(object?)x!);
}

public interface IPair
{
    object? Value { get; }
    object? Key { get; }
}

public interface IPair<out T, out TKey> : IPair
{
    new T Value { get; }
    new TKey Key { get; }
}

public record Pair<T, TKey>(T Value, TKey Key) : IPair<T, TKey>
{
    object? IPair.Value => Value;
    object? IPair.Key => Key;
}


public interface ICollectionChange
{
    IEnumerable<IPair> Removals { get; }
    IEnumerable<IPair> Additions { get; }
}

public interface ICollectionChange<out T, out TKey> : ICollectionChange
{
    new IEnumerable<IPair<T, TKey>> Removals { get; }
    new IEnumerable<IPair<T, TKey>> Additions { get; }
}

public record CollectionChange<T, TKey>(IEnumerable<IPair<T, TKey>> Removals, IEnumerable<IPair<T, TKey>> Additions) : ICollectionChange<T, TKey>
{
    IEnumerable<IPair> ICollectionChange.Removals => Removals;
    IEnumerable<IPair> ICollectionChange.Additions => Additions;
}

public record struct CollectionChange(ItemChangeKind Kind, object? Item)
    {
        public CollectionChange<T> Cast<T>()
            => new(Kind, (T)Item!);
    }

public record struct CollectionChange<T>(ItemChangeKind Kind, T Item);