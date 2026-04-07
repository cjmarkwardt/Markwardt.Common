namespace Markwardt;

public class Flag : IRecyclable
{
    private static readonly Pool<Flag> pool = new(() => new());

    public static Flag New()
        => pool.Get();

    private Flag() { }

    private bool isSet;
    public bool IsSet => isSet;

    public void Set()
        => isSet = true;

    public void Recycle()
    {
        isSet = false;
        pool.Recycle(this);
    }
}