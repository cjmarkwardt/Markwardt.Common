namespace Markwardt;

public static class TagExtensions
{
    private static readonly ConditionalWeakTable<object, object?> tags = [];

    public static object? GetTag(this object? obj)
        => obj is not null && tags.TryGetValue(obj, out object? tag) ? tag : null;

    public static void SetTag(this object obj, object? tag)
        => tags.AddOrUpdate(obj, tag);
}