namespace Markwardt;

public interface IValueWindow
{
    int Next(int? current);
    bool IsNext(int? current, int value);
}

public class ValueWindow(int max, int range) : IValueWindow
{
    public ValueWindow(int max)
        : this(max, max / 4) { }

    public int Next(int? current)
    {
        int next = current is null ? 0 : current.Value + 1;
        if (next > max)
        {
            next = 0;
        }

        return next;
    }

    public bool IsNext(int? current, int value)
        => current is null
        || (value < current && current - value > range)
        || (value > current && value - current < range);
}