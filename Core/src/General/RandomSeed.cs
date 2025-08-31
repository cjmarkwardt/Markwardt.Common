namespace Markwardt;

public record RandomSeed(string Protocol, List<int> Seeds);

public interface IRandomManager : IRandomSource
{
    void SetProtocol(string protocol, Factory factory);

    delegate IRandomizer Factory(params object?[] seeds);
}

public interface IRandomSource
{
    IRandomizer CreateRandomizer(string protocol, params object?[] seeds);
}

public static class RandomSourceExtensions
{
    public static IRandomizer CreateRandomizer(this IRandomSource source, RandomSeed seed, params int[] otherSeeds)
        => source.CreateRandomizer(seed.Protocol, seed.Seeds.Concat(otherSeeds).OfType<object?>());

    public static IRandomizer CreateRandomizer(this IRandomSource source, RandomSeed seed, params RandomSeed[] otherSeeds)
        => source.CreateRandomizer(seed, otherSeeds.SelectMany(x => x.Seeds).ToArray());
}

public class RandomManager : IRandomManager
{
    private readonly Dictionary<string, IRandomManager.Factory> factories = [];

    public void SetProtocol(string protocol, IRandomManager.Factory factory)
        => factories[protocol] = factory;

    public IRandomizer CreateRandomizer(string protocol, params object?[] seeds)
        => factories[protocol](seeds);
}