namespace Markwardt;

public interface IContentPacket<T>
{
    T GetContent();
    void SetContent(T content);
}

public static class RequestProtocolExtensions
{
    public static IMessageRequester? GetRequester(this IMessageSender sender)
        => MessageInterceptor.GetInterceptors(sender).OfType<IMessageRequester>().FirstOrDefault();

    public static async ValueTask<T> Request<T>(this IMessageSender<T> sender, T content, TimeSpan? timeout = null, CancellationToken cancellation = default)
    {
        IMessageRequester? requester = sender.GetRequester() ?? throw new InvalidOperationException("Sender does not support requests");
        return (await requester.Request(Message.New(content), timeout, cancellation)).GetContent<T>();
    }
}

public class RequestProtocol<T> : IMessageProtocol<T, T>
    where T : IRequestPacket
{
    public IMessageProcessor<T, T> CreateProcessor()
        => new Processor();

    private sealed class Processor : ValueHeaderProcessor<T, int>
    {
        protected override InspectValueKey<int> ValueHeaderKey => RequestIdKey.Instance;

        protected override void SetValueHeader(T content, int header)
            => content.SetRequest(header);

        protected override Maybe<int> GetValueHeader(T content)
        {
            int request = content.GetRequest();
            return request == 0 ? default : request.Maybe();
        }

        protected override IMessageInterceptor? CreateInterceptor()
            => new Interceptor();

        private sealed class Interceptor : MessageInterceptor, IMessageRequester
        {
            private readonly RequestManager requester = new();

            public async ValueTask<Message> Request(Message message, TimeSpan? timeout, CancellationToken cancellation)
            {
                IRequestManager.IOutgoingRequest request = requester.CreateRequest();

                message.Reliability = Reliability.Reliable;
                message.SetInspect(RequestIdKey.Instance, request.RequestId);

                Sender.Send(message);
                return await request.GetResponse(timeout, cancellation);
            }

            protected override IEnumerable<Message>? Intercept(Message message)
            {
                int requestId = message.Inspect(RequestIdKey.Instance).ValueOr(0);
                if (requestId != 0)
                {
                    if (requester.Receive(requestId, message) is IRequestManager.IIncomingRequest request)
                    {
                        message.Responder = new TransformedSender(Sender, response =>
                        {
                            response.Reliability = Reliability.Reliable;
                            response.SetInspect(RequestIdKey.Instance, -requestId);
                            return response;
                        });

                        return [message];
                    }
                    else
                    {
                        return [];
                    }
                }
                
                return null;
            }
        }
    }
}

/*public class RequestProtocol<T>() : HeaderProtocol<T, int>(MessageParameters.RequestId)
{
    protected override IMessageInterceptor? CreateInterceptor()
        => new Interceptor();

    private sealed class Interceptor : MessageInterceptor, IMessageRequester
    {
        private readonly RequestManager requester = new();

        public async ValueTask<Message> Request(Message message, TimeSpan? timeout, CancellationToken cancellation)
        {
            IRequestManager.IOutgoingRequest request = requester.CreateRequest();
            Sender.Send(message.SetParameter(MessageParameters.RequestId, request.RequestId).SetParameter(MessageParameters.Reliability, Reliability.Reliable));
            return await request.GetResponse(timeout, cancellation);
        }

        protected override IEnumerable<Message>? Intercept(Message message)
        {
            int requestId = message.GetParameter(MessageParameters.RequestId);
            if (requestId != 0)
            {
                if (requester.Receive(requestId, message) is IRequestManager.IIncomingRequest request)
                {
                    return [message.SetResponder(new TransformedSender(Sender, x => x.SetParameter(MessageParameters.RequestId, -requestId).SetParameter(MessageParameters.Reliability, Reliability.Reliable)))];
                }
                else
                {
                    return [];
                }
            }
            
            return null;
        }
    }
}*/