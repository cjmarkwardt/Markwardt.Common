namespace Markwardt;

public interface IImageAtlasEntry
{
    Rect2I Region { get; }
    Rect2 Area { get; }
}

public interface IImageAtlas
{
    Image Image { get; }

    void Extend(IEnumerable<Vector2I> sizes);
    IImageAtlasEntry Write(IPixelProjection pixels);
}

public interface IPixelMap
{
    Vector2I Size { get; }

    Color Get(Vector2I position);
}

public interface IMutablePixelMap : IPixelMap
{
    void Set(Vector2I position, Color color);
}

public class PixelMap(Vector2I size) : IMutablePixelMap
{
    public PixelMap(Color color)
        : this(Vector2I.One)
        => Set(new(0, 0), color);

    private readonly Color[,] pixels = new Color[size.X, size.Y];

    public Vector2I Size => size;

    public Color Get(Vector2I position)
        => pixels[position.X, position.Y];

    public void Set(Vector2I position, Color color)
        => pixels[position.X, position.Y] = color;
}

public static class PixelMapExtensions
{
    public static IEnumerable<Vector2I> GetPositions(this IPixelMap pixels)
    {
        for (int x = 0; x < pixels.Size.X; x++)
        {
            for (int y = 0; y < pixels.Size.Y; y++)
            {
                yield return new(x, y);
            }
        }
    }

    public static IMutablePixelMap Duplicate(this IPixelMap pixels)
    {
        PixelMap copy = new(pixels.Size);
        copy.Transform((x, _) => pixels.Get(x));
        return copy;
    }

    public static void Transform(this IMutablePixelMap pixels, Func<Vector2I, Color, Color> transform)
    {
        foreach (Vector2I position in pixels.GetPositions())
        {
            pixels.Set(position, transform(position, pixels.Get(position)));
        }
    }

    public static void WriteTo(this IPixelMap pixels, IMutablePixelMap destination, Vector2I offset = default)
    {
        for (int x = 0; x < pixels.Size.X; x++)
        {
            for (int y = 0; y < pixels.Size.Y; y++)
            {
                Vector2I position = new(x, y);
                Color color = pixels.Get(position);
                if (color.A > 0)
                {
                    destination.Set(offset + position, color);
                }
            }
        }
    }

    public static void WriteTo(this IPixelMap pixels, Image destination, Vector2I offset = default)
        => pixels.WriteTo(destination.AsPixels(), offset);
}

public class ImagePixelMap(Image image) : IMutablePixelMap
{
    public Vector2I Size => image.GetSize();

    public Color Get(Vector2I position)
        => image.GetPixelv(position);

    public void Set(Vector2I position, Color color)
        => image.SetPixelv(position, color);
}

public class ImageAtlas : IImageAtlas
{
    private int currentX;

    public Image Image { get; private set; } = new();

    /*public Image Image
    {
        get
        {
            if (image is null || regenerate)
            {
                regenerate = false;
                
                int globalX;
                if (image is null)
                {
                    globalX = 1;
                    image = Image.CreateEmpty(size.X, size.Y, false, Image.Format.Rgba8);
                }
                else
                {
                    globalX = image.GetWidth() + 1;
                    Image oldImage = image;
                    image = Image.CreateEmpty(size.X, size.X, false, Image.Format.Rgba8);
                    oldImage.AsPixels().WriteTo(image.AsPixels());
                }

                foreach (Entry entry in pendingEntries)
                {
                    for (int x = 0; x < entry.Pixels.Size.X; x++)
                    {
                        for (int y = 0; y < entry.Pixels.Size.Y; y++)
                        {
                            image.SetPixelv(entry.GetPosition(new(x, y)), entry.Pixels.Get(new(x, y)));
                        }
                    }

                    image.SetPixelv(entry.GetPosition(new(-1, -1)), entry.Pixels.Get(new(0, 0)));
                    image.SetPixelv(entry.GetPosition(new(entry.Region.Size.X, -1)), entry.Pixels.Get(new(entry.Region.Size.X - 1, 0)));
                    image.SetPixelv(entry.GetPosition(new(entry.Region.Size.X, entry.Region.Size.Y)), entry.Pixels.Get(new(entry.Region.Size.X - 1, entry.Region.Size.Y - 1)));
                    image.SetPixelv(entry.GetPosition(new(-1, entry.Region.Size.Y)), entry.Pixels.Get(new(0, entry.Region.Size.Y - 1)));

                    for (int x = 0; x < entry.Pixels.Size.X; x++)
                    {
                        image.SetPixelv(entry.GetPosition(new(x, -1)), entry.Pixels.Get(new(x, 0)));
                        image.SetPixelv(entry.GetPosition(new(x, entry.Region.Size.Y)), entry.Pixels.Get(new(x, entry.Region.Size.Y - 1)));
                    }

                    for (int y = 0; y < entry.Pixels.Size.Y; y++)
                    {
                        image.SetPixelv(entry.GetPosition(new(-1, y)), entry.Pixels.Get(new(0, y)));
                        image.SetPixelv(entry.GetPosition(new(entry.Region.Size.X, y)), entry.Pixels.Get(new(entry.Region.Size.X - 1, y)));
                    }

                    globalX += entry.Pixels.Size.X + 1;
                }
            }

            pendingEntries.Clear();

            return image;
        }
    }*/

    public IImageAtlasEntry Write(IPixelProjection pixels)
    {
        Vector2I size = pixels.Size + (Vector2I.One * 2);

        if (currentX + size.X > Image.GetWidth() || size.Y > Image.GetHeight())
        {
            throw new InvalidOperationException();
        }

        Vector2I position = new(currentX + 1, 1);

        pixels.Write(Image.AsMap(), position);
        
        Image.SetPixelv(position + new Vector2I(-1, -1), pixels.Get(new(0, 0)));
        Image.SetPixelv(position + new Vector2I(pixels.Size.X, -1), pixels.Get(new(pixels.Size.X - 1, 0)));
        Image.SetPixelv(position + new Vector2I(pixels.Size.X, pixels.Size.Y), pixels.Get(new(pixels.Size.X - 1, pixels.Size.Y - 1)));
        Image.SetPixelv(position + new Vector2I(-1, pixels.Size.Y), pixels.Get(new(0, pixels.Size.Y - 1)));

        for (int x = 0; x < pixels.Size.X; x++)
        {
            Image.SetPixelv(position + new Vector2I(x, -1), pixels.Get(new(x, 0)));
            Image.SetPixelv(position + new Vector2I(x, pixels.Size.Y), pixels.Get(new(x, pixels.Size.Y - 1)));
        }

        for (int y = 0; y < pixels.Size.Y; y++)
        {
            Image.SetPixelv(position + new Vector2I(-1, y), pixels.Get(new(0, y)));
            Image.SetPixelv(position + new Vector2I(pixels.Size.X, y), pixels.Get(new(pixels.Size.X - 1, y)));
        }

        /*Image.SetPixelv(entry.GetPosition(new(-1, -1)), pixels.Get(new(0, 0)));
        Image.SetPixelv(entry.GetPosition(new(entry.Region.Size.X, -1)), pixels.Get(new(entry.Region.Size.X - 1, 0)));
        Image.SetPixelv(entry.GetPosition(new(entry.Region.Size.X, entry.Region.Size.Y)), pixels.Get(new(entry.Region.Size.X - 1, entry.Region.Size.Y - 1)));
        Image.SetPixelv(entry.GetPosition(new(-1, entry.Region.Size.Y)), pixels.Get(new(0, entry.Region.Size.Y - 1)));

        for (int x = 0; x < pixels.Size.X; x++)
        {
            Image.SetPixelv(entry.GetPosition(new(x, -1)), pixels.Get(new(x, 0)));
            Image.SetPixelv(entry.GetPosition(new(x, entry.Region.Size.Y)), pixels.Get(new(x, entry.Region.Size.Y - 1)));
        }

        for (int y = 0; y < pixels.Size.Y; y++)
        {
            Image.SetPixelv(entry.GetPosition(new(-1, y)), pixels.Get(new(0, y)));
            Image.SetPixelv(entry.GetPosition(new(entry.Region.Size.X, y)), pixels.Get(new(entry.Region.Size.X - 1, y)));
        }*/

        currentX += size.X;
        return new Entry(this, new(position, pixels.Size));
    }

    public void Extend(IEnumerable<Vector2I> sizes)
    {
        Image? oldImage = Image;
        Image = Image.CreateEmpty(Image.GetWidth() + sizes.Select(x => x.X + 2).Sum(), Math.Max(Image.GetHeight(), sizes.Select(x => x.Y + 2).Max()), false, Image.Format.Rgba8);
        oldImage?.AsPixels().WriteTo(Image.AsPixels());
    }

    private record Entry(ImageAtlas Generator, Rect2I Region) : IImageAtlasEntry
    {
        public Rect2 Area => new((Vector2)Region.Position / Generator.Image.GetSize(), (Vector2)Region.Size / Generator.Image.GetSize());

        public Vector2I GetPosition(Vector2I position)
            => Region.Position + position;
    }
}