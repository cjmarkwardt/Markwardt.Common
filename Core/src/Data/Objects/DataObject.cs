namespace Markwardt;

public interface IDataObject
{
    Type DataType { get; }
    IDictionary<string, object> DataProperties { get; }

    IObservable<object?> Watch(string property);
    IObservable<CollectionChange> WatchItems(string property);
}

public static class DataObjectExtensions
{
    private static string GetMember(LambdaExpression selector)
        => selector.Body is MemberExpression member ? member.Member.Name : throw new NotSupportedException(selector.Body.GetType().ToString());

    public static IObservable<TValue> Watch<T, TValue>(this T target, Expression<Func<T, TValue>> selector)
        where T : IDataObject
        => target.Watch(GetMember(selector))!.Cast<TValue>();

    public static IObservable<CollectionChange<TItem>> WatchItems<T, TItem>(this T target, Expression<Func<T, ICollection<TItem>>> selector)
        where T : IDataObject
        => target.WatchItems(GetMember(selector))!.Select(x => x.Cast<TItem>());
}

public class DataObject : DynamicObject, IDataObject
{
    public static T Create<T>()
        where T : class, IDataObject
        => Impromptu.ActLike<T>(new DataObject(typeof(T)), typeof(IDataObject));

    public static Type GetDataType(object instance)
        => instance is IDataObject obj ? obj.DataType : instance.GetType();

    public DataObject(Type dataType)
    {
        DataObjectLayout layout = DataObjectLayout.Get(dataType);

        DataType = layout.DataType;

        foreach (DataObjectProperty property in layout.Properties.Values)
        {
            if (property.CollectionType is not null)
            {
                DataProperties[property.Name] = property.CollectionType.Create();
            }
        }
    }

    private readonly Dictionary<string, Subject<object?>> subjects = [];
    private readonly Dictionary<string, Subject<CollectionChange>> itemSubjects = [];

    public Type DataType { get; }

    public IDictionary<string, object> DataProperties { get; } = new Dictionary<string, object>();

    public IObservable<object?> Watch(string property)
    {
        if (!subjects.TryGetValue(property, out Subject<object?>? subject))
        {
            subject = new();
            subjects.Add(property, subject);
        }

        return subject;
    }

    public IObservable<CollectionChange> WatchItems(string property)
    {
        if (!itemSubjects.TryGetValue(property, out Subject<CollectionChange>? subject))
        {
            subject = new();
            itemSubjects.Add(property, subject);

            if (DataProperties.TryGetValue(property, out object? value) && value is IDataCollection accessor)
            {
                accessor.Watch().Subscribe(subject);
            }
            else
            {
                throw new InvalidOperationException($"Property {property} is not a data collection");
            }
        }

        return subject;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        result = DataProperties[binder.Name];
        return true;
    }

    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        if (value is null)
        {
            DataProperties.Remove(binder.Name);
        }
        else
        {
            DataProperties[binder.Name] = value;
        }

        subjects.GetValueOrDefault(binder.Name)?.OnNext(value);

        return true;
    }

    public override string ToString()
        => $"DataObject({DataType})";
}