namespace Markwardt;

public interface IImageGenerator
{
    ValueTask<BinaryData> GenerateImage(float quality, string input);
}