namespace Markwardt;

public interface IDataObjectLayout
{
    Type DataType { get; }
    IReadOnlyDictionary<string, DataObjectProperty> Properties { get; }
    IReadOnlyDictionary<int, DataObjectProperty> IndexedProperties { get; }
}

public class DataObjectLayout : IDataObjectLayout
{
    private static readonly Dictionary<Type, DataObjectLayout> layouts = [];

    public static DataObjectLayout Get(Type dataType)
    {
        if (!layouts.TryGetValue(dataType, out DataObjectLayout? layout))
        {
            layout = new(dataType);
            layouts[dataType] = layout;
        }

        return layout;
    }

    private DataObjectLayout(Type dataType)
    {
        DataType = dataType;
        Properties = dataType.GetProperties().Select((property, index) => (property, index)).ToDictionary(x => x.property.Name, x => new DataObjectProperty(x.index, x.property.Name, x.property.PropertyType));
        IndexedProperties = Properties.ToDictionary(x => x.Value.Index, x => x.Value);
    }

    public Type DataType { get; }
    public IReadOnlyDictionary<string, DataObjectProperty> Properties { get; }
    public IReadOnlyDictionary<int, DataObjectProperty> IndexedProperties { get; }
}