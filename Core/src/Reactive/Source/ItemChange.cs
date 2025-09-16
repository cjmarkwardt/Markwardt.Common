namespace Markwardt;

public record struct ItemChange<T>(ItemChangeKind Kind, T Item)
{
    public ItemChange<TConverted> Convert<TConverted>(Func<T, TConverted> convert)
        => new(Kind, convert(Item));

    public ItemChange<TCasted> Cast<TCasted>()
        => Convert(x => (TCasted)(object?)x!);
}