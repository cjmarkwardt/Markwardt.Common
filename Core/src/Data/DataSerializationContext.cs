namespace Markwardt;

public interface IDataSerializationContext
{
    void Collect(object? value);
    ValueTask Serialize(IDataWriter writer, object? value, CancellationToken cancellation = default);
}

public class DataSerializationContext(IDataSerializerSource serializers) : IDataSerializationContext
{
    private readonly ReferenceCollector references = new();

    public void Collect(object? value)
    {
        if (value is not null && value.GetType().IsClass && serializers.GetTypeId(DataObject.GetDataType(value)) is int typeId && serializers.GetSerializer(typeId) is IDataSerializer serializer)
        {
            if (serializer.IsCollectible)
            {
                if (references.Collect(value))
                {
                    serializer.Collect(this, value);
                }
            }
        }
    }

    public async ValueTask Serialize(IDataWriter writer, object? value, CancellationToken cancellation = default)
    {
        if (! await writer.TryWriteValue(value, cancellation))
        {
            if (references.Retrieve(value!, out int? reference))
            {
                int typeId = serializers.GetTypeId(DataObject.GetDataType(value!)).ValueNotNull($"No type ID found for type {value!.GetType()}");
                await writer.WriteObject(typeId, reference, cancellation);
                await serializers.GetSerializer(typeId).NotNull($"No serializer found for type {value.GetType()}").Serialize(this, writer, value, cancellation);
            }
            else
            {
                await writer.WriteReference(reference.Value, cancellation);
            }
        }
    }

    private sealed class ReferenceCollector
    {
        private readonly HashSet<object> pending = [];
        private readonly Dictionary<object, int> references = [];
        private readonly HashSet<object> retrieved = [];

        private int nextReference = 0;

        public bool Collect(object value)
        {
            if (pending.Remove(value))
            {
                references[value] = nextReference++;
            }
            else if (!references.ContainsKey(value))
            {
                pending.Add(value);
                return true;
            }

            return false;
        }

        public bool Retrieve(object value, [NotNullWhen(false)] out int? reference)
        {
            if (references.TryGetValue(value, out int existingReference))
            {
                reference = existingReference;
                return retrieved.Add(value);
            }

            reference = null;
            return true;
        }
    }
}