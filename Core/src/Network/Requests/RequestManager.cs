namespace Markwardt;

public interface IRequestManager
{
    IOutgoingRequest CreateRequest();
    IIncomingRequest? Receive(int requestId, Message message);

    interface IOutgoingRequest
    {
        int RequestId { get; }

        ValueTask<Message> GetResponse(TimeSpan? timeout = null, CancellationToken cancellation = default);
    }
    
    interface IIncomingRequest
    {
        int ResponseId { get; }
        Message Message { get; }
    }
}

public class RequestManager : IRequestManager
{
    private readonly IdSet requestIds = new(1);
    private readonly Dictionary<int, OutgoingRequest> outgoingRequests = [];

    public IRequestManager.IOutgoingRequest CreateRequest()
    {
        OutgoingRequest request = new(this);
        outgoingRequests.Add(request.RequestId, request);
        return request;
    }

    public IRequestManager.IIncomingRequest? Receive(int requestId, Message message)
    {
        if (requestId > 0)
        {
            return new IncomingRequest(-requestId, message);
        }
        else if (outgoingRequests.TryGetValue(-requestId, out OutgoingRequest? request))
        {
            request.SetResponse(message);
        }

        return null;
    }

    private sealed class OutgoingRequest(RequestManager requester) : IRequestManager.IOutgoingRequest
    {
        private readonly TaskCompletionSource<Message> completion = new();

        public int RequestId { get; } = requester.requestIds.Next();

        public void SetResponse(Message response)
            => completion.SetResult(response);

        public async ValueTask<Message> GetResponse(TimeSpan? timeout = null, CancellationToken cancellation = default)
        {
            try
            {
                return timeout.HasValue ? await completion.Task.WaitAsync(timeout.Value, cancellation) : await completion.Task.WaitAsync(cancellation);
            }
            finally
            {
                requester.requestIds.Release(RequestId);
                requester.outgoingRequests.Remove(RequestId);
            }
        }
    }

    private sealed record IncomingRequest(int ResponseId, Message Message) : IRequestManager.IIncomingRequest;
}