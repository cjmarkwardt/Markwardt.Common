namespace Markwardt;

public interface IDataDeserializationContext
{
    ValueTask<object?> Deserialize(IDataReader reader, CancellationToken cancellation = default);
}

public class DataDeserializationContext(IDataSerializerSource serializers) : IDataDeserializationContext
{
    private readonly DataReferenceResolver references = new();

    public async ValueTask<object?> Deserialize(IDataReader reader, CancellationToken cancellation = default)
    {
        object? value = await reader.Read(cancellation);
        if (value is DataObjectSignal objectSignal)
        {
            value = await serializers.GetSerializer(objectSignal.TypeId).NotNull($"No serializer found for type ID {objectSignal.TypeId}").Deserialize(this, reader, cancellation);

            if (objectSignal.Reference is not null)
            {
                references.Set(objectSignal.Reference.Value, value);
            }

            return value;
        }
        else if (value is DataReferenceSignal referenceSignal)
        {
            return references.Resolve(referenceSignal.Reference);
        }
        else
        {
            return value;
        }
    }
}