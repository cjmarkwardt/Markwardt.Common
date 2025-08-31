namespace Markwardt;

public interface IRandomizer
{
    double GetNumber(double min, double max);
    int GetInteger(int min, int max);
}

public static class RandomizerExtensions
{
    public static double GetNumber(this IRandomizer randomizer, double max)
        => randomizer.GetNumber(0, max);

    public static double GetNumber(this IRandomizer randomizer)
        => randomizer.GetNumber(double.MaxValue);

    public static double GetPercent(this IRandomizer randomizer)
        => randomizer.GetNumber(0, 1);
        
    public static int GetInteger(this IRandomizer randomizer, int max)
        => randomizer.GetInteger(0, max);

    public static int GetInteger(this IRandomizer randomizer)
        => randomizer.GetInteger(int.MaxValue);

    public static bool Test(this IRandomizer randomizer, double chance)
        => randomizer.GetPercent() > chance;
}

public class Randomizer(params object?[] seeds) : IRandomizer
{
    private static int CombineSeeds(IEnumerable<object?> seeds)
    {
        HashCode hash = new();
        foreach (object? seed in seeds)
        {
            hash.Add(seed);
        }

        return hash.ToHashCode();
    }

    private readonly Random random = seeds.Length == 0 ? new() : new(CombineSeeds(seeds));

    public double GetNumber(double min, double max)
        => (min + (max - min)) * random.NextDouble();

    public int GetInteger(int min, int max)
        => random.Next(min, max);
}