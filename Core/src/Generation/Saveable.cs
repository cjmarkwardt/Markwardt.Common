namespace Markwardt;

public interface ISaveable
{
    ValueTask Save(Stream output);
}

public static class SaveableExtensions
{
    public static async ValueTask SaveFile(this ISaveable saveable, string file)
    {
        await using FileStream output = File.OpenWrite(file);
        await saveable.Save(output);
    }
}