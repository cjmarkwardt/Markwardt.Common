namespace Markwardt;

public class GodotAsset<T>(string path) : IAsset<T>
    where T : class
{
    public async ValueTask<T> Load(CancellationToken cancellation)
    {
        Error error = ResourceLoader.LoadThreadedRequest(path);
        if (error is not Error.Ok)
        {
            throw new AssetLoadException(path, error.ToString());
        }

        while (true)
        {
            ResourceLoader.ThreadLoadStatus status = ResourceLoader.LoadThreadedGetStatus(path);
            if (status is ResourceLoader.ThreadLoadStatus.Loaded)
            {
                return (T)(object)ResourceLoader.LoadThreadedGet(path);
            }
            else if (status is ResourceLoader.ThreadLoadStatus.Failed || status is ResourceLoader.ThreadLoadStatus.InvalidResource)
            {
                throw new AssetLoadException(path, status.ToString());
            }

            await Task.Delay(10, cancellation);
        }
    }
}