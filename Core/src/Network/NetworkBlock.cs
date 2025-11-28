namespace Markwardt;

public interface INetworkBlock
{
    NetworkReliability Reliability { get; }
    ReadOnlyMemory<byte> Data { get; }

    void MarkSending();
    void MarkSent();
    void MarkComplete();
}