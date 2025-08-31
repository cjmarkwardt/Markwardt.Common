namespace Markwardt;

public interface IReadOnlyColorMap : IReadOnlyCollection<KeyValuePair<int, Color>>
{
    Color? Get(int index);
}

public static class ReadOnlyColorMapExtensions
{
    public static Color? Get<TId>(this IReadOnlyColorMap colors, TId id)
        where TId : struct, Enum
        => colors.Get(Convert.ToInt32(id));
}

public interface IColorMap : IReadOnlyColorMap
{
    void Set(int index, Color? color);
}

public static class ColorMapExtensions
{
    public static void Set<TId>(this IColorMap colors, TId id, Color? color)
        where TId : struct, Enum
        => colors.Set(Convert.ToInt32(id), color);
}

public class ColorMap : IColorMap
{
    public static Texture2D Generate(IEnumerable<KeyValuePair<int, Color>> colors)
    {
        Image image = Image.CreateEmpty(colors.Select(x => x.Key).DefaultIfEmpty().Max() + 1, 1, false, Image.Format.Rgba8);
        foreach ((int index, Color color) in colors)
        {
            image.SetPixel(index, 0, color);
        }

        return ImageTexture.CreateFromImage(image);
    }

    private readonly Dictionary<int, Color> colors = [];

    public int Count => colors.Count;

    public void Set(int index, Color? color)
    {
        if (color is null)
        {
            colors.Remove(index);
        }
        else
        {
            colors[index] = color.Value;
        }
    }

    public Color? Get(int index)
        => colors.TryGetValue(index, out Color color) ? color : null;

    public IEnumerator<KeyValuePair<int, Color>> GetEnumerator()
        => colors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}