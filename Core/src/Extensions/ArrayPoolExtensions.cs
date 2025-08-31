namespace Markwardt;

public static class ArrayPoolExtensions
{
    public static async ValueTask<TResult> UseBuffer<TBuffer, TResult>(this ArrayPool<TBuffer> pool, int length, int maximumBufferLength, Func<Memory<TBuffer>, ValueTask<TResult>> action)
    {
        if (length > maximumBufferLength)
        {
            return await action(new TBuffer[length]);
        }
        else
        {
            TBuffer[] buffer = pool.Rent(length);
            try
            {
                return await action(buffer.AsMemory()[..length]);
            }
            finally
            {
                pool.Return(buffer);
            }
        }
    }
}