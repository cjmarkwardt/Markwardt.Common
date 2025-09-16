namespace Markwardt;

public static class ObservableTarget
{
    private static readonly Dictionary<Type, Type> implementations = new()
    {
        { typeof(IObservableValue<>), typeof(SourceValue<>) },
        { typeof(IObservableCollection<>), typeof(SourceList<>) },
        { typeof(IObservableList<>), typeof(SourceList<>) },
        { typeof(IObservableSet<>), typeof(SourceSet<>) },
        { typeof(IObservableSet<,>), typeof(SourceSet<,>) },
        { typeof(IObservableDictionary<,>), typeof(SourceDictionary<,>) },
        { typeof(ISourceValue<>), typeof(SourceValue<>) },
        { typeof(ISourceCollection<>), typeof(SourceList<>) },
        { typeof(ISourceList<>), typeof(SourceList<>) },
        { typeof(ISourceSet<>), typeof(SourceSet<>) },
        { typeof(ISourceSet<,>), typeof(SourceSet<,>) },
        { typeof(ISourceDictionary<,>), typeof(SourceDictionary<,>) }
    };

    private static readonly HashSet<Type> valueTypes = new()
    {
        { typeof(IObservableValue<>) },
        { typeof(ISourceValue<>) }
    };

    private static readonly HashSet<Type> collectionTypes = new()
    {
        { typeof(IObservableCollection<>) },
        { typeof(IObservableList<>) },
        { typeof(IObservableSet<>) },
        { typeof(IObservableSet<,>) },
        { typeof(ISourceCollection<>) },
        { typeof(ISourceList<>) },
        { typeof(ISourceSet<>) },
        { typeof(ISourceSet<,>) }
    };

    private static readonly HashSet<Type> dictionaryTypes = new()
    {
        { typeof(IObservableDictionary<,>) },
        { typeof(ISourceDictionary<,>) }
    };

    public static bool IsValue(Type type)
        => type.IsGenericType && valueTypes.Contains(type.GetGenericTypeDefinition());

    public static bool IsCollection(Type type)
        => type.IsGenericType && collectionTypes.Contains(type.GetGenericTypeDefinition());

    public static bool IsDictionary(Type type)
        => type.IsGenericType && dictionaryTypes.Contains(type.GetGenericTypeDefinition());

    public static Type? GetImplementation(Type type)
        => type.IsGenericType && implementations.TryGetValue(type.GetGenericTypeDefinition(), out Type? implementation) ? implementation.MakeGenericType(type.GetGenericArguments()) : null;

    public static Func<object> GetCreator(Type type)
    {
        Type implementation = GetImplementation(type).NotNull($"Type {type} is not an observable target");
        return () => Activator.CreateInstance(implementation, true).NotNull();
    }
}