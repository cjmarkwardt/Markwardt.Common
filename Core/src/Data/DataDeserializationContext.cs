namespace Markwardt;

public interface IDataDeserializationContext
{
    ValueTask<object?> Deserialize(IDataReader reader);
}

public class DataDeserializationContext(IDataSerializerSource serializers) : IDataDeserializationContext
{
    private readonly DataReferenceResolver references = new();

    public async ValueTask<object?> Deserialize(IDataReader reader)
    {
        object? value = await reader.Read();
        if (value is DataObjectSignal objectSignal)
        {
            value = await serializers.GetSerializer(objectSignal.TypeId).NotNull($"No serializer found for type ID {objectSignal.TypeId}").Deserialize(this, reader);

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