namespace Markwardt;

public interface IMultiBuffer<T>
{
    IDisposable<IBuffer<T>> Rent(int minCapacity = 0);
}

public class MultiBuffer<T> : IMultiBuffer<T>
{
    private readonly List<IBuffer<T>> buffers = [];

    public IDisposable<IBuffer<T>> Rent(int minCapacity = 0)
    {
        IBuffer<T>? buffer = buffers.FirstOrDefault(x => x.Capacity >= minCapacity);
        if (buffer is not null)
        {
            buffer.Reset();
            buffers.Remove(buffer);
        }
        else
        {
            buffer = new Buffer<T>();
        }

        return new Disposable<IBuffer<T>>(buffer, () => buffers.Add(buffer));
    }
}