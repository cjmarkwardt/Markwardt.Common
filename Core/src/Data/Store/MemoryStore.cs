namespace Markwardt;

public class MemoryStore : IDataStore
{
    private readonly Dictionary<string, Entry> entries = [];

    public IAsyncEnumerable<(string Id, DateTime Timestamp)> List()
        => entries.Values.Select(x => (x.Id, x.Timestamp)).ToAsyncEnumerable();

    public ValueTask<Stream> Open(string id, bool overwrite, CancellationToken cancellation = default)
    {
        Entry entry = entries[id];
        entry.UpdateTimestamp();
        entry.Stream.Position = 0;

        if (overwrite)
        {
            entry.Stream.SetLength(0);
        }

        return ValueTask.FromResult<Stream>(entry.Stream);
    }

    public ValueTask Delete(string id, CancellationToken cancellation = default)
    {
        entries.Remove(id);
        return ValueTask.CompletedTask;
    }

    private class Entry(string id)
    {
        public MemoryStream Stream { get; } = new();

        public DateTime Timestamp { get; private set; }
        
        public string Id => id;

        public void UpdateTimestamp()
            => Timestamp = DateTime.Now;
    }
}