namespace Markwardt;

public interface IWorldStore<TWorld>
{
    IAsyncEnumerable<string> List();
    ValueTask<TWorld> Load(string id, CancellationToken cancellation = default);
    ValueTask Save(string id, TWorld world, CancellationToken cancellation = default);
    ValueTask Delete(string id, CancellationToken cancellation = default);
}

public class WorldStore<TWorld>([Inject<WorldDataStoreTag>] IDataStore store, IDataSchemaSerializer serializer, [Inject<WorldSchemaTag>] string schema) : IWorldStore<TWorld>
    where TWorld : notnull
{
    public IAsyncEnumerable<string> List()
        => store.List().OrderByDescending(x => x.Timestamp).Select(x => x.Id);

    public async ValueTask<TWorld> Load(string id, CancellationToken cancellation = default)
    {
        await using Stream stream = await store.Open(id, false, cancellation);
        return (TWorld) await serializer.Deserialize(stream, cancellation);
    }

    public async ValueTask Save(string id, TWorld data, CancellationToken cancellation = default)
    {
        await using Stream stream = await store.Open(id, true, cancellation);
        await serializer.Serialize(schema, stream, data, cancellation);
    }

    public async ValueTask Delete(string id, CancellationToken cancellation = default)
        => await store.Delete(id, cancellation);
}