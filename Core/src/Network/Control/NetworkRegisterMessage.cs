namespace Markwardt;

public record NetworkRegisterMessage(string Identifier, ReadOnlyMemory<byte> Verifier, object? Details) : NetworkControlMessage(NetworkControlHeader.Register)
{
    public override void Write(INetworkSerializer serializer, IBuffer<byte> buffer)
    {
        buffer.WriteString(Identifier);
        buffer.WriteBlock(Verifier);
        WriteMessage(serializer, buffer, Details);
    }
}