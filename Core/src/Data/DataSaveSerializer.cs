namespace Markwardt;

public interface IDataSaveSerializer
{
    string? Schema { get; set; }

    ValueTask Serialize(IDataWriter writer, object value);
    ValueTask<object> Deserialize(IDataReader reader);
}

public static class DataSaveSerializerExtensions
{
    public static async ValueTask Serialize(this IDataSaveSerializer serializer, Stream output, object value)
        => await serializer.Serialize(new DataWriter(new DataPartWriter(new BlockWriter(output))), value);

    public static async ValueTask<object> Deserialize(this IDataSaveSerializer serializer, Stream input)
        => await serializer.Deserialize(new DataReader(new DataPartReader(new BlockReader(input))));
}

public class DataSaveSerializer : IDataSaveSerializer
{
    public required IDataConfiguration Configuration { get; init; }

    public string? Schema { get; set; }

    public async ValueTask Serialize(IDataWriter writer, object value)
    {
        if (Schema is null)
        {
            throw new InvalidOperationException("Cannot serialize data without a schema.");
        }

        await writer.WriteText(Schema);
        await Configuration.Serialize(Schema, writer, value);
    }

    public async ValueTask<object> Deserialize(IDataReader reader)
    {
        if (Schema is null)
        {
            throw new InvalidOperationException("Cannot deserialize data without a schema.");
        }

        return await Configuration.Deserialize(await reader.Read<string>(), reader);
    }
}