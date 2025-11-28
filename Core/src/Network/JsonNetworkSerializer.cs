namespace Markwardt;

public class JsonNetworkSerializer(JsonSerializerOptions? options = null) : ConfiguredNetworkSerializer
{
    protected override void AutoSerialize(Type type, object message, IMemoryWriteable<byte> writer)
        => writer.WriteString(JsonSerializer.Serialize(message, type, options));

    protected override object AutoDeserialize(Type type, MemoryReader<byte> reader, ReadOnlySpan<byte> data)
        => JsonSerializer.Deserialize(reader.ReadString(data), type, options).NotNull();
}