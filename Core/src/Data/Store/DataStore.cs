namespace Markwardt;

public interface IDataStore
{
    IAsyncEnumerable<(string Id, DateTime Timestamp)> List();
    ValueTask<Stream> Open(string id, bool overwrite, CancellationToken cancellation = default);
    ValueTask Delete(string id, CancellationToken cancellation = default);
}