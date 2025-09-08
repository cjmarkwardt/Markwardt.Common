namespace Markwardt;

public interface IDataSerializerSource
{
    int? GetTypeId(Type type);
    IDataSerializer? GetSerializer(int id);
}

public static class DataSerializerSourceExtensions
{
    public static async ValueTask Serialize(this IDataSerializerSource serializers, IDataWriter writer, object? value)
    {
        if (value is null)
        {
            await writer.WriteNull();
        }
        else
        {
            DataSerializationContext context = new(serializers);
            context.Collect(value);
            await context.Serialize(writer, value);
        }
    }

    public static async ValueTask<object?> Deserialize(this IDataSerializerSource serializers, IDataReader reader)
        => await new DataDeserializationContext(serializers).Deserialize(reader);
}

public class DataSerializerSource(IEnumerable<KeyValuePair<Type, IDataSerializer>> serializers) : IDataSerializerSource
{
    private readonly Dictionary<int, IDataSerializer> serializers = serializers.Select((x, i) => (x.Key, x.Value, Index: i)).ToDictionary(x => x.Index, x => x.Value);
    private readonly Dictionary<Type, int> typeIds = serializers.Select((x, i) => (x.Key, Index: i)).ToDictionary(x => x.Key, x => x.Index);

    public int? GetTypeId(Type type)
        => typeIds.TryGetValue(type is IDataObject obj ? obj.DataType : type, out int id) ? id : null;

    public IDataSerializer? GetSerializer(int id)
        => serializers.TryGetValue(id, out IDataSerializer? serializer) ? serializer : null;
}