namespace Markwardt;

public static class RangeUtils
{
    public static Range FromStart(int index, int count = 1)
        => new(Index.FromStart(index), Index.FromStart(index + count));

    public static Range FromEnd(int index, int count = 1)
        => new(Index.FromEnd(index), Index.FromEnd(index + count));
}