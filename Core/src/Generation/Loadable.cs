namespace Markwardt;

public interface ILoadable
{
    ValueTask Load(Stream input);
}

public static class LoadableExtensions
{
    public static async ValueTask LoadFile(this ILoadable loadable, string file)
    {
        await using FileStream input = File.OpenRead(file);
        await loadable.Load(input);
    }
}