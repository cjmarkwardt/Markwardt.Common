namespace Markwardt;

public static class BinaryDataExtensions
{
    public static async ValueTask ToFile(this BinaryData data, string path)
    {
        await using FileStream output = File.OpenWrite(path);
        await using Stream input = data.ToStream();
        await input.CopyToAsync(output);
    }

    public static async ValueTask ToFile(this ValueTask<BinaryData> data, string path)
    {
        await using FileStream output = File.OpenWrite(path);
        await using Stream input = (await data).ToStream();
        await input.CopyToAsync(output);
    }
}