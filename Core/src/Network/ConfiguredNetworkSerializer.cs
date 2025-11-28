namespace Markwardt;

public interface INetworkSerializerConfiguration
{
    void ConfigureType(Type type, INetworkSerializer? serializer = null, bool highPriority = false);
}

public interface IConfiguredNetworkSerializer : INetworkSerializer, INetworkSerializerConfiguration;

public abstract class ConfiguredNetworkSerializer : IConfiguredNetworkSerializer
{
    private readonly static int lowPriorityIdStart = 50;

    private int nextHighPriorityId = 0;
    private int nextLowPriorityId = lowPriorityIdStart;

    private readonly Dictionary<int, Configuration> configurationsById = [];
    private readonly Dictionary<Type, Configuration> configurationsByType = [];

    public void ConfigureType(Type type, INetworkSerializer? serializer = null, bool highPriority = false)
    {
        if (configurationsByType.ContainsKey(type))
        {
            throw new InvalidOperationException($"Type {type} is already configured for serialization");
        }

        Configuration configuration = new(type, GetNextId(highPriority), serializer);
        configurationsById.Add(configuration.Id, configuration);
        configurationsByType.Add(type, configuration);
    }

    public void Serialize(object message, IMemoryWriteable<byte> writer)
    {
        Type type = message.GetType();
        Configuration configuration = configurationsByType.GetValueOrDefault(type) ?? throw new InvalidOperationException($"Type {type} is not configured for serialization");
        
        writer.WriteVariableInteger(configuration.Id);
        
        if (configuration.Serializer is null)
        {
            AutoSerialize(type, message, writer);
        }
        else
        {
            configuration.Serializer.Serialize(message, writer);
        }
    }

    public object Deserialize(MemoryReader<byte> reader, ReadOnlySpan<byte> data)
    {
        int typeId = (int)reader.ReadVariableInteger(data);
        Configuration configuration = configurationsById.GetValueOrDefault(typeId) ?? throw new InvalidOperationException($"Type ID {typeId} is not configured for serialization");
        
        if (configuration.Serializer is null)
        {
            return AutoDeserialize(configuration.Type, reader, data);
        }
        else
        {
            return configuration.Serializer.Deserialize(reader, data);
        }
    }

    protected abstract void AutoSerialize(Type type, object message, IMemoryWriteable<byte> writer);
    protected abstract object AutoDeserialize(Type type, MemoryReader<byte> reader, ReadOnlySpan<byte> data);

    private int GetNextId(bool highPriority)
    {
        if (highPriority && nextHighPriorityId < lowPriorityIdStart)
        {
            return nextHighPriorityId++;
        }
        else
        {
            return nextLowPriorityId++;
        }
    }

    private sealed record Configuration(Type Type, int Id, INetworkSerializer? Serializer);
}