namespace Markwardt;

public interface ISummaryTextGenerationChain : ITextGenerationChain
{
    int SummaryThreshold { get; set; }
    float SummaryQuality { get; set; }
    float SummaryEffort { get; set; }
    object? SummaryIntro { get; set; }
    Type? SummaryType { get; set; }
}

public class SummaryTextGenerationChain(ITextGenerationChain chain, int summaryThreshold) : ISummaryTextGenerationChain
{
    public int SummaryThreshold { get; set; } = summaryThreshold;
    public float SummaryQuality { get; set; }
    public float SummaryEffort { get; set; }
    public object? SummaryIntro { get; set; }
    public Type? SummaryType { get; set; }

    public int Count => chain.Count;

    public async ValueTask<string> Generate(float quality, float effort, Type? outputType, params IEnumerable<string> inputs)
        => await chain.Generate(quality, effort, outputType, inputs);

    public async ValueTask Add(TextGeneration generation)
    {
        await chain.Add(generation);

        if (chain.Count > SummaryThreshold)
        {
            await Summarize();
        }
    }

    public void Remove(TextGeneration generation)
        => chain.Remove(generation);

    public void Clear()
        => chain.Clear();

    public async ValueTask Save(Stream output)
        => await chain.Save(output);

    public async ValueTask Load(Stream input)
        => await chain.Load(input);

    public IEnumerator<TextGeneration> GetEnumerator()
        => chain.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    private async ValueTask Summarize()
    {
        string summary = await chain.Generate(SummaryQuality, SummaryEffort, SummaryType, "Summarize everything so far");

        chain.Clear();

        if (SummaryIntro is not null)
        {
            await chain.Add(TextGeneration.Command(SummaryIntro));
        }

        await chain.Add(TextGeneration.Command(summary));
    }
}