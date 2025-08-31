namespace Markwardt;

public static class ImageExtensions
{
    public static IMutablePixelMap AsPixels(this Image image)
        => new ImagePixelMap(image);
}