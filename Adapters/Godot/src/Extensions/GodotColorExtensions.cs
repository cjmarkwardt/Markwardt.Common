namespace Markwardt;

public static class GodotColorExtensions
{
    public static Color WithAlpha(this Color color, float alpha)
        => new(color, alpha);

    public static Color Grayscale(this Color value)
    {
        float grayscale = (value.R + value.G + value.B) / 3;
        return new Color(grayscale, grayscale, grayscale, value.A);
    }

    public static Color Blend(this Color value, Color color)
        => new(value.R + color.R / 2, value.G + color.G / 2, value.B + color.B / 2, value.A);

    public static Color Colorize(this Color value, Color color)
    {
        if (value.R != value.B || value.B != value.G)
        {
            value = value.Grayscale();
        }

        return value.Blend(color);
    }

    public static Color ApplyHslOffset(this Color color, Color hslOffset)
        => (Color)((HslColor)color).ColorizeAsOffset(hslOffset);
}