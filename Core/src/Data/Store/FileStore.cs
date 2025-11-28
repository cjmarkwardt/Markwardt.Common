namespace Markwardt;

[DefaultImplementation<FileStore>]
public delegate IDataStore FileStoreFactory(string folder, string fileExtension);

public class FileStore(string folder, string fileExtension) : IDataStore
{
    public IAsyncEnumerable<(string Id, DateTime Timestamp)> List()
        => new DirectoryInfo(folder).EnumerateFiles($"*.{fileExtension}", SearchOption.TopDirectoryOnly).Select(x => (Path.GetRelativePath(folder, x.FullName), x.LastWriteTime)).ToAsyncEnumerable();

    public async ValueTask<Stream> Open(string id, bool overwrite, CancellationToken cancellation = default)
        => await Task.Run(() => File.Open(GetPath(id), overwrite ? FileMode.Create : FileMode.Open, FileAccess.ReadWrite), cancellation);

    public async ValueTask Delete(string id, CancellationToken cancellation = default)
        => await Task.Run(() => File.Delete(GetPath(id)), cancellation);

    private string GetPath(string id)
        => Path.Combine(folder, id);
}