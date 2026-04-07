namespace Markwardt;

public interface ITextGenerationChain : IEnumerable<TextGeneration>, ISaveable, ILoadable
{
    int Count { get; }

    ValueTask<string> Generate(float quality, float effort, Type? outputType, params IEnumerable<string> inputs);
    ValueTask Add(TextGeneration generation);
    void Remove(TextGeneration generation);
    void Clear();
}

public static class TextGenerationChainExtensions
{
    public static async ValueTask Add(this ITextGenerationChain chain, params IEnumerable<TextGeneration> generations)
    {
        foreach (TextGeneration generation in generations)
        {
            await chain.Add(generation);
        }
    }

    public static int GetLength(this ITextGenerationChain chain)
        => chain.Sum(x => x.Value.Length);

    public static async ValueTask<object> GenerateObject(this ITextGenerationChain chain, float quality, float effort, Type outputType, params IEnumerable<object> inputs)
        => TextGeneration.Deserialize(outputType, await chain.Generate(quality, effort, outputType, TextGeneration.Serialize(inputs)));

    public static async ValueTask<T> Generate<T>(this ITextGenerationChain chain, float quality, float effort, params IEnumerable<object> inputs)
        => (T) await chain.GenerateObject(quality, effort, typeof(T), inputs);

    public static async ValueTask Command(this ITextGenerationChain chain, params IEnumerable<object> inputs)
        => await chain.Add(TextGeneration.Command(inputs));

    public static async ValueTask<string> Request(this ITextGenerationChain chain, float quality, float effort, Type? outputType, params IEnumerable<string> inputs)
    {
        await chain.Add(TextGeneration.Request(inputs));
        return await chain.Generate(quality, effort, outputType);
    }

    public static async ValueTask<object> RequestObject(this ITextGenerationChain chain, float quality, float effort, Type? outputType, params IEnumerable<object> inputs)
        => TextGeneration.Deserialize(outputType, await chain.Request(quality, effort, outputType, TextGeneration.Serialize(inputs)));

    public static async ValueTask<T> Request<T>(this ITextGenerationChain chain, float quality, float effort, params IEnumerable<object> inputs)
        => (T) await chain.RequestObject(quality, effort, typeof(T), inputs);
}

public class TextGenerationChain(ITextGenerator generator) : ITextGenerationChain
{
    private List<TextGeneration> generations = [];

    public int Count => generations.Count;

    public async ValueTask<string> Generate(float quality, float effort, Type? outputType, params IEnumerable<string> inputs)
        => await generator.GenerateText(quality, effort, outputType, generations.Concat(TextGeneration.Request(inputs)));

    public ValueTask Add(TextGeneration generation)
    {
        generations.Add(generation);
        return ValueTask.CompletedTask;
    }

    public void Remove(TextGeneration generation)
        => generations.Remove(generation);

    public void Clear()
        => generations.Clear();

    public async ValueTask Save(Stream output)
        => await JsonSerializer.SerializeAsync(output, generations);

    public async ValueTask Load(Stream input)
        => generations = await JsonSerializer.DeserializeAsync<List<TextGeneration>>(input) ?? throw new InvalidOperationException();

    public IEnumerator<TextGeneration> GetEnumerator()
        => generations.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();
}