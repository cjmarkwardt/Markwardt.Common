namespace Markwardt.Network;

public static class RequestProtocolExtensions
{
    public static IRequestManager? GetRequestManager<T>(this IConnection<T> connection)
        => NetworkInterceptor.GetInterceptors(connection).OfType<IRequestManager>().FirstOrDefault();

    public static async ValueTask<T> Request<T>(this IConnection<T> connection, T content, TimeSpan? timeout = null, CancellationToken cancellation = default)
    {
        IRequestManager? requester = connection.GetRequestManager() ?? throw new InvalidOperationException("Sender does not support requests");
        return (await requester.Request(Packet.New(content), timeout, cancellation)).As<T>().Content;
    }
}

public class RequestProtocol<T> : IConnectionProtocol<T, T>
    where T : IHeaderPacket<RequestHeader>
{
    public IConnectionProcessor<T, T> CreateProcessor()
        => new Processor();

    private sealed class Processor : HeaderProcessor<T, RequestHeader>
    {
        public Processor()
            => interceptor = new Interceptor(this);

        private readonly Interceptor interceptor;

        protected override IEnumerable<INetworkInterceptor> Interceptors => base.Interceptors.Concat([interceptor]);

        protected override void OnDisconnected(Exception? exception)
        {
            base.OnDisconnected(exception);

            interceptor.Dispose();
        }

        private sealed class Interceptor(Processor processor) : NetworkInterceptor, IRequestManager
        {
            private readonly Requester requests = new();

            public async ValueTask<Packet> Request(Packet packet, TimeSpan? timeout, CancellationToken cancellation)
            {
                IRequest request = requests.CreateRequest();

                packet.Reliability = Reliability.Reliable;
                processor.SetHeader(packet, new(RequestFlow.Request, request.RequestId));

                Send(packet);
                return await request.GetResponse(timeout, cancellation);
            }
            
            protected override IEnumerable<Packet>? Intercept(Packet packet)
            {
                if (processor.GetHeader(packet).TryGetValue(out RequestHeader header))
                {
                    if (header.Flow is RequestFlow.Request)
                    {
                        packet.Responder = response =>
                        {
                            response.Reliability = Reliability.Reliable;
                            processor.SetHeader(response, new RequestHeader(RequestFlow.Response, header.Id));
                            Send(response);
                        };

                        return [packet];
                    }
                    else if (header.Flow is RequestFlow.Response)
                    {
                        requests.ReceiveResponse(header.Id, packet);
                        return [];
                    }
                }
                
                return null;
            }
        }
    }
}