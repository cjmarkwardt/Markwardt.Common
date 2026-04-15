using System.Runtime.Serialization;
using System.Text.Json;
using Markwardt.Network;

namespace Markwardt;

public class SandboxStarter : IStarter
{
    [DataContract]
    public record Test([property: DataMember(Order = 1)] string Value, [property: DataMember(Order = 2)] int Priority = 0)
    {
        private Test()
            : this(default!, default) { }
    }

    public async ValueTask Start(CancellationToken cancellation = default)
    {
        IConnectionProtocol<Test, ReadOnlyMemory<byte>> protocol = new ConnectionProtocol<Test>()
            .Configure(packet =>
            {
                packet.Priority = packet.Content.Priority;
                packet.Reliability = Reliability.Ordered;
            })
            .AsStandardMessages()
            .AsJson()
            .AsBytes()
            .WithInterrupts(5)
            .AsProtobuf()
            .WithLengthPrefixBuffer();

        IHostHandler<Test> CreateHandler<T>(string name, Func<Test, Test> onRequest, Action<IConnection<Test>>? onConnected = null)
            => new HostHandler<Test>()
            {
                ConnectedHandler = connection =>
                {
                    Console.WriteLine($"{name}/Connected");
                    connection.GetReceivedChannels().Subscribe(x =>
                    {
                        Console.WriteLine($"{name}/Channel {x.Message.Content.Value}");
                        x.Messages.Subscribe(y => Console.WriteLine($"{name}/Channel {y.Content.Value}"));
                    });
                    onConnected?.Invoke(connection);
                },
                DisconnectedHandler = (connection, exception) => Console.WriteLine($"{name}/Disconnected {exception?.Message ?? "Closed"}"),
                ReceivedHandler = (connection, packet) =>
                {
                    Console.WriteLine($"{name}/Received {packet.Content.Value}");
                    
                    if (packet.CanRespond)
                    {
                        packet.Respond(onRequest(packet.Content));
                    }
                },
                SignalReceivedHandler = (connection, packet) => Console.WriteLine($"{name}/SignalReceived {packet.Signal}")
            };

        using IDisposable host = protocol.HostTcp(out int port).Handle(CreateHandler<Test>("Host", x => new($"Replying to {x.Value} from Host")));

        int sent = 0;
        using IDisposable client = protocol.Configure(packet => sent += packet.Content.Length).ConnectTcp("localhost", port).Handle(CreateHandler<Test>("Client", x => new($"Replying to {x.Value} from Client"), async connection =>
        {
            connection.Send(new Test("Hello here is a test"));
            connection.Send(new Test("High priority test", 1));
            connection.Send(new Test("Max priority", 2));
            Console.WriteLine((await connection.Request(new Test("REQUEST"))).Value);
            connection.Send(new Test("Another test"));
            IChannelValue<int> value = connection.OpenChannelValue(TimeSpan.FromSeconds(1), new("HAIL"), 0, x => new(x.ToString()), TimeSpan.FromSeconds(2));
            value.Value = 2;
            value.Value = 3;
            Console.WriteLine(sent);

            await Task.Delay(5000);

            connection.Dispose();
        }));

        await Task.Delay(-1);
    }
}