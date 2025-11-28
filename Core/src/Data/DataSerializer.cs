namespace Markwardt;

public interface IDataSerializer
{
    bool IsCollectible { get; }

    void Collect(IDataSerializationContext context, object value);
    ValueTask Serialize(IDataSerializationContext context, IDataWriter writer, object value, CancellationToken cancellation = default);
    ValueTask<object> Deserialize(IDataDeserializationContext context, IDataReader reader, CancellationToken cancellation = default);
}

public abstract class DataSerializer<T> : IDataSerializer
    where T : notnull
{
    public virtual bool IsCollectible => false;

    public virtual void Collect(IDataSerializationContext context, T value) { }

    public abstract ValueTask Serialize(IDataSerializationContext context, IDataWriter writer, T value, CancellationToken cancellation = default);
    public abstract ValueTask<T> Deserialize(IDataDeserializationContext context, IDataReader reader, CancellationToken cancellation = default);

    void IDataSerializer.Collect(IDataSerializationContext context, object value)
        => Collect(context, (T)value);

    async ValueTask IDataSerializer.Serialize(IDataSerializationContext context, IDataWriter writer, object value, CancellationToken cancellation)
        => await Serialize(context, writer, (T)value, cancellation);

    async ValueTask<object> IDataSerializer.Deserialize(IDataDeserializationContext context, IDataReader reader, CancellationToken cancellation)
        => await Deserialize(context, reader, cancellation);
}