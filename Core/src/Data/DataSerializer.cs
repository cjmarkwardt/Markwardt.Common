namespace Markwardt;

public interface IDataSerializer
{
    bool IsCollectible { get; }

    void Collect(IDataSerializationContext context, object value);
    ValueTask Serialize(IDataSerializationContext context, IDataWriter writer, object value);
    ValueTask<object> Deserialize(IDataDeserializationContext context, IDataReader reader);
}

public abstract class DataSerializer<T> : IDataSerializer
    where T : notnull
{
    public virtual bool IsCollectible => false;

    public virtual void Collect(IDataSerializationContext context, T value) { }

    public abstract ValueTask Serialize(IDataSerializationContext context, IDataWriter writer, T value);
    public abstract ValueTask<T> Deserialize(IDataDeserializationContext context, IDataReader reader);

    void IDataSerializer.Collect(IDataSerializationContext context, object value)
        => Collect(context, (T)value);

    async ValueTask IDataSerializer.Serialize(IDataSerializationContext context, IDataWriter writer, object value)
        => await Serialize(context, writer, (T)value);

    async ValueTask<object> IDataSerializer.Deserialize(IDataDeserializationContext context, IDataReader reader)
        => await Deserialize(context, reader);
}