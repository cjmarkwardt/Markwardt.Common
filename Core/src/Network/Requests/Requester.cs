namespace Markwardt.Network;

public interface IRequester
{
    IRequest CreateRequest();
    void ReceiveResponse(int id, Packet packet);
}

public class Requester : IRequester
{
    private readonly IdSet requestIds = new();
    private readonly Dictionary<int, Request> outgoingRequests = [];

    public IRequest CreateRequest()
    {
        Request request = new(this);
        outgoingRequests.Add(request.RequestId, request);
        return request;
    }

    public void ReceiveResponse(int id, Packet packet)
        => outgoingRequests.GetValueOrDefault(id)?.SetResponse(packet);

    private sealed class Request(Requester manager) : IRequest
    {
        private readonly TaskCompletionSource<Packet> completion = new();

        public int RequestId { get; } = manager.requestIds.Next();

        public void SetResponse(Packet response)
            => completion.SetResult(response);

        public async ValueTask<Packet> GetResponse(TimeSpan? timeout = null, CancellationToken cancellation = default)
        {
            try
            {
                return timeout.HasValue ? await completion.Task.WaitAsync(timeout.Value, cancellation) : await completion.Task.WaitAsync(cancellation);
            }
            finally
            {
                manager.requestIds.Release(RequestId);
                manager.outgoingRequests.Remove(RequestId);
            }
        }
    }
}