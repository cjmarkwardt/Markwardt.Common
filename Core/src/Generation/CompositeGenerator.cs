namespace Markwardt;

public class CompositeGenerator(ITextGenerator textGenerator, IImageGenerator imageGenerator, ISpeechGenerator speechGenerator) : IGenerator
{
    public async ValueTask<string> GenerateText(float quality, float effort, Type? outputType, params IEnumerable<TextGeneration> inputs)
        => await textGenerator.GenerateText(quality, effort, outputType, inputs);

    public async ValueTask<BinaryData> GenerateImage(float quality, string input)
        => await imageGenerator.GenerateImage(quality, input);

    public async ValueTask<BinaryData> GenerateSpeech(float quality, VoiceType type, float voice, string input, string script)
        => await speechGenerator.GenerateSpeech(quality, type, voice, input, script);
}