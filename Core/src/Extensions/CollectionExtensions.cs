namespace Markwardt;

public static class CollectionExtensions
{
    public static void Add<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            collection.Add(item);
        }
    }

    public static void Remove<T>(this ICollection<T> collection, IEnumerable<T> items)
    {
        foreach (T item in items)
        {
            collection.Remove(item);
        }
    }

    public static int GetPercentageIndex<T>(this IReadOnlyCollection<T> collection, float value)
        => Math.Clamp((int)Math.Floor(value * collection.Count), 0, collection.Count - 1);

    public static T GetPercentageItem<T>(this IReadOnlyList<T> list, float value)
        => list[list.GetPercentageIndex(value)];
}