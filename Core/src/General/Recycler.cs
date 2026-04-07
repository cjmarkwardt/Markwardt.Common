namespace Markwardt;

public static class Recycler
{
    public static Recycler<T> New<T>(T state, Action<T> recycle)
        => Recycler<T>.New(state, recycle);
}

public class Recycler<T> : IRecyclable
{
    private readonly static Pool<Recycler<T>> pool = new(() => new());

    public static Recycler<T> New(T state, Action<T> recycle)
    {
        Recycler<T> recycler = pool.Get();
        recycler.state = state.Maybe();
        recycler.recycle = recycle.Maybe();
        return recycler;
    }

    private Recycler() { }

    private Maybe<T> state;
    private Maybe<Action<T>> recycle;

    public void Recycle()
    {
        recycle.Value(state.Value);

        state = default;
        recycle = default;

        pool.Recycle(this);
    }
}