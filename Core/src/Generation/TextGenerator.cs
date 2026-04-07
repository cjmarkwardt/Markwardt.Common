namespace Markwardt;

public interface ITextGenerator
{
    ValueTask<string> GenerateText(float quality, float effort, Type? outputType, params IEnumerable<TextGeneration> inputs);
}

public static class TextGeneratorExtensions
{
    public static async ValueTask<object> GenerateObject(this ITextGenerator generator, float quality, float effort, Type outputType, params IEnumerable<object> inputs)
        => TextGeneration.Deserialize(outputType, await generator.GenerateText(quality, effort, outputType, TextGeneration.Request(inputs)));

    public static async ValueTask<T> GenerateObject<T>(this ITextGenerator generator, float quality, float effort, params IEnumerable<object> inputs)
        => (T) await generator.GenerateObject(quality, effort, typeof(T), inputs);

    public static ITextGenerationChain CreateChain(this ITextGenerator generator)
        => new TextGenerationChain(generator);

    public static ISummaryTextGenerationChain CreateSummaryChain(this ITextGenerator generator, int summaryThreshold, Type? summaryType, string? summaryIntro = null, int summaryQuality = 1, int summaryEffort = 1)
        => new SummaryTextGenerationChain(generator.CreateChain(), summaryThreshold);

    public static ISummaryTextGenerationChain CreateSummaryChain<T>(this ITextGenerator generator, int summaryThreshold, string? summaryIntro = null, int summaryQuality = 1, int summaryEffort = 1)
        => generator.CreateSummaryChain(summaryThreshold, typeof(T), summaryIntro, summaryQuality, summaryEffort);
}