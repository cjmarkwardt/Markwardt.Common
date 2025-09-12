namespace Markwardt;

public interface IDataObject
{
    Type DataType { get; }
    IReadOnlyDictionary<string, object> DataProperties { get; }
}

public class DataObject(DataObjectLayout layout) : DynamicObject, IDataObject
{
    public static T Create<T>()
        where T : class, IDataObject
        => Impromptu.ActLike<T>(new DataObject(typeof(T)), typeof(IDataObject));

    public static Type GetDataType(object instance)
        => instance is IDataObject obj ? obj.DataType : instance.GetType();

    public DataObject(Type dataType)
        : this(DataObjectLayout.Get(dataType)) { }

    public Type DataType => layout.DataType;

    public IReadOnlyDictionary<string, object> DataProperties { get; } = layout.Properties.Values.ToDictionary(x => x.Name, x => x.Create());

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        result = DataProperties[binder.Name];
        return true;
    }

    public override string ToString()
        => $"DataObject({DataType})";
}