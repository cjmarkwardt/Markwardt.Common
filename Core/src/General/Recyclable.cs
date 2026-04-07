namespace Markwardt;

public interface IRecyclable
{
    void Recycle();
}

public interface IRecyclable<T> : IRecyclable
{
    T Value { get; }
}

public class Recyclable<T>(T value, IRecyclable recycler) : IRecyclable<T>
{
    public T Value => value;

    public void Recycle()
        => recycler.Recycle();
}