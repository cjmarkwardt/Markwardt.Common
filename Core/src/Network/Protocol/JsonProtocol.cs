namespace Markwardt.Network;

public class JsonProtocol<T>(JsonSerializerOptions? options = null) : IConnectionProtocol<T, string>
{
    public IConnectionProcessor<T, string> CreateProcessor()
        => new Processor(options ?? new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault });

    private sealed class Processor(JsonSerializerOptions options) : ConvertProcessor<T, string>
    {
        protected override string Convert(T value)
            => JsonSerializer.Serialize(value, options);

        protected override T Revert(string value)
            => JsonSerializer.Deserialize<T>(value, options).NotNull();
    }
}