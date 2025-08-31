namespace Markwardt;

public interface IPixelProjection
{
    Vector2I Size { get; }

    Color Get(Vector2I position);
}

public static class PixelProjection
{
    public static IPixelProjection Color(Color color)
        => new ColorProjection(color);

    public static IPixelProjection Template(ColorTemplate template)
        => new ColorProjection(template.ToEncodedColor());

    public static IPixelProjection Create(Vector2I size, Color color = default)
        => Color(color).Resize(size);
}

public interface IPixelMapp : IPixelProjection
{
    void Set(Vector2I position, Color color);
}

public static class PixelProjectionExtensions
{
    public static Rect2I GetArea(this IPixelProjection source)
        => new(Vector2I.Zero, source.Size);

    public static IPixelProjection Resize(this IPixelProjection source, Func<Vector2I, Vector2I> resize)
        => new ResizeProjection(source, resize);

    public static IPixelProjection Resize(this IPixelProjection source, Vector2I size)
        => source.Resize(_ => size);

    public static IPixelProjection Offset(this IPixelProjection source, Vector2I offset)
        => new OffsetProjection(source, offset);

    public static IPixelProjection OffsetX(this IPixelProjection source, int x)
        => new OffsetProjection(source, Vector2I.Right * x);

    public static IPixelProjection OffsetY(this IPixelProjection source, int y)
        => new OffsetProjection(source, Vector2I.Down * y);

    public static IPixelProjection Transform(this IPixelProjection source, Func<Vector2I, Color, Color> transform)
        => new TransformProjection(source, transform);

    public static IPixelProjection Transform(this IPixelProjection source, Func<Color, Color> transform)
        => new TransformProjection(source, (_, color) => transform(color));

    public static IPixelProjection Combine(this IPixelProjection source, IPixelProjection other, Func<Vector2I, Color, Color, Color> combine)
        => source.Transform((position, color) => combine(position, color, other.Get(position)));

    public static IPixelProjection Combine(this IPixelProjection source, IPixelProjection other, Func<Color, Color, Color> combine)
        => source.Combine(other, (_, color, otherColor) => combine(color, otherColor));

    public static IPixelProjection Grayscale(this IPixelProjection source)
        => source.Transform(color => color.Grayscale());

    public static IPixelProjection Blend(this IPixelProjection source, Color other)
        => source.Transform(color => color.Blend(other));

    public static IPixelProjection Blend(this IPixelProjection source, IPixelProjection other)
        => source.Combine(other, (color, otherColor) => color.Blend(otherColor));

    public static IPixelProjection ColorizeTemplate(this IPixelProjection source, IEnumerable<Color> colors)
        => source.Transform(color => ColorTemplate.FromEncodedColor(color).Colorize(colors));

    public static IPixelProjection OverlayClipped(this IPixelProjection source, IPixelProjection overlay, float alphaCutoff = 0)
        => source.Combine(overlay, (color, otherColor) => otherColor.A > alphaCutoff ? otherColor : color);

    //public static IPixelProjection AlphaBlend(this IPixelProjection source, Color)

    public static IPixelProjection Colorize(this IPixelProjection source, Color color)
        => source.Transform(color => color.Colorize(color));

    public static IPixelMapp AsMap(this Image image)
        => new ImageMap(image);

    public static void Write(this IPixelProjection source, IPixelMapp destination, Vector2I offset = default)
    {
        for (int x = 0; x < source.Size.X; x++)
        {
            for (int y = 0; y < source.Size.Y; y++)
            {
                Vector2I position = new(x, y);
                destination.Set(offset + position, source.Get(position));
            }
        }
    }
}

public record ColorProjection(Color Color) : IPixelProjection
{
    public Vector2I Size => Vector2I.One;

    public Color Get(Vector2I position)
        => Color;
}

public record ResizeProjection(IPixelProjection Source, Func<Vector2I, Vector2I> Resize) : IPixelProjection
{
    public Vector2I Size => Resize(Source.Size);

    public Color Get(Vector2I position)
        => Source.Get(position);
}



public record OffsetProjection(IPixelProjection Source, Vector2I Offset) : IPixelProjection
{
    public Vector2I Size => Source.Size;

    public Color Get(Vector2I position)
        => Source.Get(position - Offset);
}

public record ImageMap(Image Image, Color Space = default) : IPixelMapp
{
    public Vector2I Size => Image.GetSize();

    public Color Get(Vector2I position)
    {
        if (this.GetArea().HasPoint(position))
        {
            return Image.GetPixelv(position);
        }
        else
        {
            return Space;
        }
    }

    public void Set(Vector2I position, Color color)
    {
        if (this.GetArea().HasPoint(position))
        {
            Image.SetPixelv(position, color);
        }
    }
}

public record TransformProjection(IPixelProjection Source, Func<Vector2I, Color, Color> Transform) : IPixelProjection
{
    public Vector2I Size => Source.Size;

    public Color Get(Vector2I position)
        => Transform(position, Source.Get(position));
}

public static class Hue
{
    public static float Red => 0;
    public static float Orange => 1f / 12;
    public static float Yellow => 2f / 12;
    public static float Green => 4f / 12;
    public static float Cyan => 6f / 12;
    public static float Blue => 8f / 12;
    public static float Purple => 9f / 12;
    public static float Pink => 10f / 12;
}

public record struct Chroma(float Hue, float Saturation = 1)
{
    public readonly HslColor ToColor(float luminosity = 0.5f, float alpha = 1)
        => new(Hue, Saturation, luminosity, alpha);

    public readonly override string ToString()
        => $"({Hue}, {Saturation})";
}

public record struct HslColor(float Hue, float Saturation = 1, float Luminosity = 0.5f, float Alpha = 1)
{
    public static implicit operator HslColor(Color color)
    {
        float min = Math.Min(Math.Min(color.R, color.G), color.B);
        float max = Math.Max(Math.Max(color.R, color.G), color.B);
        float delta = max - min;

        float hue = 0;
        float saturation = 0;
        float lightness = (max + min) / 2;

        if (delta != 0)
        {
            if (lightness < 0.5f)
            {
                saturation = delta / (max + min);
            }
            else
            {
                saturation = delta / (2 - max - min);
            }

            if (color.R == max)
            {
                hue = (color.G - color.B) / delta;
            }
            else if (color.G == max)
            {
                hue = 2 + (color.B - color.R) / delta;
            }
            else if (color.B == max)
            {
                hue = 4 + (color.R - color.G) / delta;
            }
        }

        return new(hue / 6, saturation, lightness, color.A);
    }

    public static implicit operator Color(HslColor color)
    {
        float r;
        float g;
        float b;

        if (color.Saturation == 0)
        {
            r = g = b = color.Luminosity;
        }
        else
        {
            float t1, t2;
            
            if (color.Luminosity < 0.5f)
            {
                t2 = color.Luminosity * (1 + color.Saturation);
            }
            else
            {
                t2 = (color.Luminosity + color.Saturation) - (color.Luminosity * color.Saturation);
            }

            t1 = 2 * color.Luminosity - t2;

            float Calculate(float color)
            {
                if (color < 0) { color += 1; }
                if (color > 1) { color -= 1; }
                if (color * 6 < 1) { return t1 + (t2 - t1) * 6 * color; }
                if (color * 2 < 1) { return t2; }
                if (color * 3 < 2) { return t1 + (t2 - t1) * (2f/3 - color) * 6; }
                return t1;
            }

            r = Calculate(color.Hue + (1f/3));
            g = Calculate(color.Hue);
            b = Calculate(color.Hue - (1f/3));
        }

        return new(r, g, b, color.Alpha);
    }

    public readonly Chroma Chroma => new(Hue, Saturation);

    public readonly HslColor ColorizeAsOffset(Color targetColor)
        => new((Hue + targetColor.R) % 1, (Saturation + targetColor.B) % 1, (Luminosity + targetColor.G) % 1);

    public override readonly string ToString()
        => $"({Hue}, {Saturation}, {Luminosity}, {Alpha})";
}

public readonly record struct GradientColor(Color Start, Color End)
{
    public GradientColor(string start, string end)
        : this(new Color(start), new Color(end)) { }
}

/*public readonly record struct ColorTemplate(int Index, float Alpha)
{
    public static ColorTemplate FromEncodedColor(Color color)
        => new(Mathf.RoundToInt(color.R * 16) + (16 * Mathf.RoundToInt(color.G * 16)) + (16 * 16 * Mathf.RoundToInt(color.B * 16)), color.A);

    public readonly Color Resolve(IEnumerable<Color> colors)
        => colors.ElementAt(Index).WithAlpha(Alpha);
}*/

public readonly record struct ColorTemplate(int StartIndex, int EndIndex, float Gradient, float Alpha)
{
    public static ColorTemplate Black => new(0, 0, 0, 1);
    public static ColorTemplate White => new(1, 0, 0, 1);

    public static ColorTemplate FromEncodedColor(Color color)
        => new(color.R8, color.G8, color.B, color.A);

    public readonly Color Colorize(IEnumerable<Color> colors)
    {
        colors = colors.Prepend(Colors.White).Prepend(Colors.Black);
        return colors.ElementAtOrDefault(StartIndex).Lerp(colors.ElementAtOrDefault(EndIndex), Gradient).WithAlpha(Alpha);
    }

    public Color ToEncodedColor()
        => new(StartIndex / 255f, EndIndex / 255f, Gradient, Alpha);
}