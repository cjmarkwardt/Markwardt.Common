namespace Markwardt;

public interface IDataConfiguration
{
    void Configure(string schema, IDataSerializerSource serializers);
    void ConfigureUpgrade(string schema, string targetSchema, Func<object, ValueTask<object>> upgrade);
    ValueTask Serialize(string schema, IDataWriter writer, object value);
    ValueTask<object> Deserialize(string schema, IDataReader reader, bool upgrade = true);
}

public class DataConfiguration : IDataConfiguration
{
    private readonly Dictionary<string, IDataSerializerSource> configurations = [];
    private readonly Dictionary<string, UpgradePath> upgradePaths = [];

    public void Configure(string schema, IDataSerializerSource serializers)
        => configurations[schema] = serializers;

    public void ConfigureUpgrade(string schema, string targetSchema, Func<object, ValueTask<object>> upgrade)
        => upgradePaths[schema] = new(targetSchema, upgrade);

    public async ValueTask Serialize(string schema, IDataWriter writer, object value)
    {
        if (!configurations.TryGetValue(schema, out IDataSerializerSource? serializers))
        {
            throw new KeyNotFoundException($"No serializer configured for schema '{schema}'.");
        }

        await serializers.Serialize(writer, value);
    }

    public async ValueTask<object> Deserialize(string schema, IDataReader reader, bool upgrade = true)
    {
        if (!configurations.TryGetValue(schema, out IDataSerializerSource? serializers))
        {
            throw new KeyNotFoundException($"No serializer configured for schema '{schema}'.");
        }

        object value = (await serializers.Deserialize(reader)).NotNull();

        if (upgrade)
        {
            while (upgradePaths.TryGetValue(schema, out UpgradePath? upgradePath))
            {
                value = await upgradePath.Upgrade(value);
                schema = upgradePath.TargetSchema;
            }
        }

        return value;
    }

    private record SchemaConfiguration(Type Type, IDataSerializerSource Serializers);
    private record UpgradePath(string TargetSchema, Func<object, ValueTask<object>> Upgrade);
}