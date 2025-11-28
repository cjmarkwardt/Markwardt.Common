namespace Markwardt;

public interface IDataSchemaChainConfigurer<T>
{
    IDataSchemaChainConfigurer<TNext> NextVersion<TNext>(IDataSerializerSource source, AsyncFunction<T, TNext> upgrade)
        where TNext : class, IDataObject;

    IDataSchemaSerializer CreateSerializer();
}

public class DataSchemaChainConfigurer<T>(string prefix, int version, IDictionary<string, IDataSerializerSource> schemas, IDictionary<string, DataUpgrade> upgrades) : IDataSchemaChainConfigurer<T>
    where T : class, IDataObject
{
    private static string GetSchemaId(string prefix, int version)
        => $"{prefix}:{version}";

    public static DataSchemaChainConfigurer<T> Start(string prefix, int version, IDataSerializerSource source)
        => new(prefix, version, new Dictionary<string, IDataSerializerSource> { [GetSchemaId(prefix, version)] = source }, new Dictionary<string, DataUpgrade>());

    public IDataSchemaChainConfigurer<TNext> NextVersion<TNext>(IDataSerializerSource source, AsyncFunction<T, TNext> upgrade)
        where TNext : class, IDataObject
    {
        int nextVersion = version + 1;
        schemas.Add(GetSchemaId(nextVersion), source);
        upgrades.Add(GetSchemaId(nextVersion - 1), new DataUpgrade(GetSchemaId(nextVersion), async (obj, cancellation) => await upgrade((T)obj, cancellation)));
        return new DataSchemaChainConfigurer<TNext>(prefix, nextVersion, schemas, upgrades);
    }

    public IDataSchemaSerializer CreateSerializer()
        => new DataSchemaSerializer(schemas.AsReadOnly(), new DataUpgrader(upgrades.AsReadOnly()));

    private string GetSchemaId(int version)
        => GetSchemaId(prefix, version);
}

[ServiceType<IDictionary<string, IDataSerializerSource>>]
public class DataSchemasTag : ConstructorTag<Dictionary<string, IDataSerializerSource>>;

public interface IDataSchemaSerializer
{
    ValueTask Serialize(string schema, IDataWriter writer, object value, CancellationToken cancellation = default);
    ValueTask<object> Deserialize(IDataReader reader, CancellationToken cancellation = default);
}

public static class DataSchemaSerializerExtensions
{
    public static async ValueTask Serialize(this IDataSchemaSerializer serializer, string schema, Stream output, object value, CancellationToken cancellation = default)
        => await serializer.Serialize(schema, new DataWriter(new DataPartWriter(new BlockWriter(output))), value, cancellation);

    public static async ValueTask<object> Deserialize(this IDataSchemaSerializer serializer, Stream input, CancellationToken cancellation = default)
        => await serializer.Deserialize(new DataReader(new DataPartReader(new BlockReader(input))), cancellation);
}

public class DataSchemaSerializer([Inject<DataSchemasTag>] IReadOnlyDictionary<string, IDataSerializerSource> schemas, IDataUpgrader? upgrader = null) : IDataSchemaSerializer
{
    public static IDataSchemaChainConfigurer<T> Chain<T>(string prefix, int version, IDataSerializerSource source)
        where T : class, IDataObject
        => DataSchemaChainConfigurer<T>.Start(prefix, version, source);

    public async ValueTask Serialize(string schema, IDataWriter writer, object value, CancellationToken cancellation = default)
    {
        if (!schemas.TryGetValue(schema, out IDataSerializerSource? serializers))
        {
            throw new KeyNotFoundException($"No serializer configured for schema '{schema}'.");
        }

        await writer.WriteText(schema, cancellation);
        await serializers.Serialize(writer, value, cancellation);
    }

    public async ValueTask<object> Deserialize(IDataReader reader, CancellationToken cancellation = default)
    {
        string schema = await reader.Read<string>(cancellation);
        if (!schemas.TryGetValue(schema, out IDataSerializerSource? serializers))
        {
            throw new KeyNotFoundException($"No serializer configured for schema '{schema}'.");
        }

        object value = (await serializers.Deserialize(reader, cancellation)).NotNull();
        if (upgrader is not null)
        {
            value = await upgrader.Upgrade(schema, value, cancellation);
        }

        return (schema, value);
    }
}