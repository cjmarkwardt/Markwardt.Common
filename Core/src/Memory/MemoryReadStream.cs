namespace Markwardt;

public class MemoryReadStream(ReadOnlyMemory<byte> memory) : BaseMemoryReadStream
{
    public MemoryReadStream()
        : this(ReadOnlyMemory<byte>.Empty) { }

    private ReadOnlyMemory<byte> memory = memory;
    public ReadOnlyMemory<byte> Memory
    {
        get => memory;
        set
        {
            memory = value;
            Position = 0;
        }
    }

    protected override ReadOnlyMemory<byte> GetMemory()
        => Memory;
}