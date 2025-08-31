namespace Markwardt;

public static class TimeInstanceExtensions
{
    public static float GetElapsed(this TimeInstance time)
        => time.GetTicksMsec() / 1000f;
}