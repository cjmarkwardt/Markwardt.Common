namespace Markwardt;

public static class ArrayPoolExtensions
{
    public static async ValueTask<TResult> UseBuffer<TBuffer, TResult>(this ArrayPool<TBuffer> pool, int length, Func<Memory<TBuffer>, ValueTask<TResult>> action, int? maximumBufferLength = null)
    {
        if (maximumBufferLength is not null && length > maximumBufferLength)
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

    public static async ValueTask UseBuffer<TBuffer>(this ArrayPool<TBuffer> pool, int length, Func<Memory<TBuffer>, ValueTask> action, int? maximumBufferLength = null)
        => await pool.UseBuffer(length, async buffer =>
        {
            await action(buffer);
            return false;
        }, maximumBufferLength);
}