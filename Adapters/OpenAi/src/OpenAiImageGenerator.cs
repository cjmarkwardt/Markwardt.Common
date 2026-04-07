namespace Markwardt;

public class OpenAiImageGenerator(string apiKey, IEnumerable<string> models) : IImageGenerator
{
    private readonly ClientModelSet<ImageClient> models = new(models, model => new(model, apiKey));

    public async ValueTask<BinaryData> GenerateImage(float quality, string input)
        => (await models.GetClient(quality).GenerateImageAsync(input, new()
        {
            #pragma warning disable OPENAI001
            OutputFileFormat = GeneratedImageFileFormat.Png
            #pragma warning restore OPENAI001
        })).Value.ImageBytes;
}