namespace Markwardt;

public class JsonConverter<T> : IConverter<ReadOnlyMemory<byte>, T>
{
    public T Convert(ReadOnlyMemory<byte> value)
        => System.Text.Json.JsonSerializer.Deserialize<T>(value.Span) ?? throw new InvalidOperationException($"JSON deserialization failed for type {typeof(T)}.");

    public ReadOnlyMemory<byte> Revert(T value)
        => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(value);
}