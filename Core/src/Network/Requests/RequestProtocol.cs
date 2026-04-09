namespace Markwardt;

public static class RequestProtocolExtensions
{
    public static IRequestManager? GetRequestManager(this IMessageSender sender)
        => MessageInterceptor.GetInterceptors(sender).OfType<IRequestManager>().FirstOrDefault();

    public static async ValueTask<T> Request<T>(this IMessageSender<T> sender, T content, TimeSpan? timeout = null, CancellationToken cancellation = default)
    {
        IRequestManager? requester = sender.GetRequestManager() ?? throw new InvalidOperationException("Sender does not support requests");
        return (await requester.Request(Message.New(content), timeout, cancellation)).GetContent<T>();
    }
}

public class RequestProtocol<T> : IMessageProtocol<T, T>
    where T : IHeaderPacket<RequestHeader>
{
    public IMessageProcessor<T, T> CreateProcessor()
        => new Processor();

    private sealed class Processor : HeaderrProcessor<T, RequestHeader>
    {
        public Processor()
            => interceptor = new Interceptor(this);

        private readonly Interceptor interceptor;

        protected override IEnumerable<IMessageInterceptor> Interceptors => base.Interceptors.Concat([interceptor]);

        protected override void OnDisconnected(Exception? exception)
        {
            base.OnDisconnected(exception);

            interceptor.Dispose();
        }

        private sealed class Interceptor(Processor processor) : MessageInterceptor, IRequestManager
        {
            private readonly Requester requests = new();

            public async ValueTask<Message> Request(Message message, TimeSpan? timeout, CancellationToken cancellation)
            {
                IRequest request = requests.CreateRequest();

                message.Reliability = Reliability.Reliable;
                processor.SetHeader(message, new(RequestFlow.Request, request.RequestId));

                Sender.Send(message);
                return await request.GetResponse(timeout, cancellation);
            }

            protected override IEnumerable<Message>? Intercept(Message message)
            {
                if (processor.GetHeader(message).TryGetValue(out RequestHeader header))
                {
                    if (header.Flow is RequestFlow.Request)
                    {
                        message.Responder = new TransformedSender(Sender, response =>
                        {
                            response.Reliability = Reliability.Reliable;
                            processor.SetHeader(response, new RequestHeader(RequestFlow.Response, header.Id));
                            return response;
                        });

                        return [message];
                    }
                    else if (header.Flow is RequestFlow.Response)
                    {
                        requests.ReceiveResponse(header.Id, message);
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