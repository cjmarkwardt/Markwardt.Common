namespace Markwardt;

public class SandboxStarter : IStarter, INetworkHandler
{
    public NetworkManager Network { get; private set; } = default!;

    public async ValueTask Start(CancellationToken cancellation = default)
    {
        int port = 50050;

        //GameManager game = new(new FileStore("worlds", "wld"));

        //Network = new(this, new TextSerializer());
        /*Network.Host(new LiteNetHoster(port), "HOST");
        await Network.Connect(new LiteNetConnector("localhost", port), profile: "CLIENT", request: "itsa me!", isSecure: true);*/
        /*Network.Host(new TcpBlockHoster(port), "HOST");
        await Network.Connect(new TcpBlockConnector("localhost", port), request: "itsa me!", profile: "CLIENT");
        //await Network.Connect(request: "itsa me!", profile: "CLIENT");*/

        /*GameManager game = new(new FileStore(@"/home/cjmarkwardt/Projects/Markwardt.Hazelnut/Common/Sandbox/Worlds", "wld"));
        game.Create("MyWorld", string.Empty);
        await game.Server!.Join("Bob");
        await game.Player!.Connection.Send("hello from client!");
        await Task.Delay(1000);
        await game.Leave("bye bye");*/

        await Task.Delay(-1);

        Console.WriteLine("Sandbox");
    }

    ValueTask<object?> INetworkHandler.OnRegistration(INetworkHost? host, string identifier, ReadOnlyMemory<byte> verifier, object? details, bool isLocal, CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }

    ValueTask<(object? UserProfile, ReadOnlyMemory<byte> Verifier)> INetworkHandler.OnAuthentication(INetworkHost? host, string identifier, bool isLocal, CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }

    ValueTask<(object? Response, object? ConnectionProfile)> INetworkHandler.OnConnection(INetworkHost? host, NetworkUser? user, object? request, bool isLocal, bool isSecure, CancellationToken cancellation)
    {
        return ValueTask.FromResult<(object?, object?)>(("itsa you!", "SERVER"));
    }

    async void INetworkHandler.OnConnected(INetworkConnection connection, object? message)
    {
        Console.WriteLine($"{connection.Profile}: CONNECTED {message}");

        //await connection.Send("hello!", NetworkSecurity.TrySecure);

        if (connection.Profile is "CLIENT")
        {
            //Console.WriteLine($"{connection.Profile}: RESPONDED {await connection.Request("woah")}");
            INetworkChannel channel1 = connection.OpenChannel("channel1");
            INetworkGroupChannel channel2 = Network.CreateGroupChannel("channel2");
            channel2.Open(connection);
            /*await Task.Delay(1000);
            channel1.Update("woot");
            await Task.Delay(1000);
            channel1.Update("woot");
            channel2.Update("a");
            await Task.Delay(1000);
            channel1.Update("wooter");*/
            channel2.Update("b");
            channel2.Update("c");
            channel2.Update("d");
            channel2.Update("e");
            channel2.Update("f");
            channel2.Update("g");
            channel2.Update("h");
            channel2.Update("i");
            channel2.Update("j");
            channel2.Update("k");
            channel2.Update("l");
            channel2.Update("m");
            channel2.Update("n");
            channel2.Update("o");
            channel2.Update("p");
            channel2.Update("q");
            //channel2.Dispose();
            await Task.Delay(-1);
            await connection.Disconnect("bye!");
        }
    }

    void INetworkHandler.OnReceived(INetworkConnection connection, object? channel, object message)
    {
        Console.WriteLine($"{connection.Profile}: RECEIVED {message} ({channel})");
    }

    ValueTask<(object Response, NetworkSecurity? Security, INetworkBlockPool? Pool)> INetworkHandler.OnRequested(INetworkConnection connection, object message, CancellationToken cancellation)
    {
        Console.WriteLine($"{connection.Profile}: REQUESTED {message}");
        return ValueTask.FromResult<(object, NetworkSecurity?, INetworkBlockPool?)>((message is "woah" ? "yo" : "nope", null, null));
    }

    (object ChannelProfile, NetworkSecurity? Security, INetworkBlockPool? Pool) INetworkHandler.OnChannelOpened(INetworkConnection connection, object message)
    {
        Console.WriteLine($"{connection.Profile}: CHANNEL OPENED {message}");
        return (message, null, null);
    }

    void INetworkHandler.OnChannelClosed(INetworkConnection connection, object? profile)
    {
        Console.WriteLine($"{connection.Profile}: CHANNEL CLOSED {profile}");
    }

    void INetworkHandler.OnDisconnected(INetworkConnection connection, Exception? exception)
    {
        Console.WriteLine($"{connection.Profile}: DISCONNECTED {exception?.Message}");
    }

    void INetworkHandler.OnRecycled(object message)
    {
        
    }

    public class GameServer : WorldServerHandler<IWorld>
    {
        protected override async ValueTask Run(CancellationToken cancellation)
        {
            await base.Run(cancellation);

            await Task.CompletedTask;
            Console.WriteLine("SERVER STARTED");
        }

        public override void OnConnected(IWorldPlayer player)
        {
            base.OnConnected(player);

            Console.WriteLine($"SERVER CONNECTED {player.Name}");
        }

        public override void OnReceived(IWorldPlayer player, object? channel, object message)
        {
            base.OnReceived(player, channel, message);

            Console.WriteLine($"SERVER RECEIVED FROM {player.Name}: {message}");
        }

        public override void OnDisconnected(IWorldPlayer player, Exception? exception)
        {
            base.OnDisconnected(player, exception);

            Console.WriteLine($"SERVER DISCONNECTED {player.Name}: {exception?.Message}");
        }

        public override void OnStopped()
        {
            base.OnStopped();

            Console.WriteLine("SERVER STOPPED");
        }
    }
    
    public class GameClient : WorldClientHandler
    {
        protected override async ValueTask Run(CancellationToken cancellation)
        {
            await base.Run(cancellation);

            await Task.CompletedTask;
            Console.WriteLine("CLIENT CONNECTED");
        }

        public override void OnReceived(object? channel, object message)
        {
            base.OnReceived(channel, message);

            Console.WriteLine($"CLIENT RECEIVED: {message}");
        }

        public override void OnDisconnected(Exception? exception)
        {
            base.OnDisconnected(exception);

            Console.WriteLine("CLIENT DISCONNECTED");
        }
    }

    public interface IWorld : IDataObject
    {
        
    }

    /*async void INetworkHandler.OnConnected(INetworkConnection connection)
    {
        Console.WriteLine($"{connection.Profile}: CONNECTED");

        connection.Send("hello!");

        if (connection.Profile is "SERVER")
        {
            Console.WriteLine($"{connection.Profile}: RESPONDED {await connection.Request("woah")}");
            await Task.Delay(2000);
            await connection.Disconnect("bye!");
        }
    }

    ValueTask INetworkHandler.OnReceived(INetworkConnection connection, INetworkChannel? channel, object message)
    {
        Console.WriteLine($"{connection.Profile}: RECEIVED {message}");
        return ValueTask.CompletedTask;
    }

    ValueTask<object> INetworkHandler.OnRequested(INetworkConnection connection, object message, CancellationToken cancellation)
    {
        Console.WriteLine($"{connection.Profile}: REQUESTED {message}");
        return ValueTask.FromResult<object>(message is "woah" ? "yo" : "nope");
    }

    void INetworkHandler.OnRecycled(INetworkConnection connection, object message)
    {
        
    }

    void INetworkHandler.OnDisconnected(INetworkConnection connection, Exception? exception)
    {
        Console.WriteLine($"{connection.Profile}: DISCONNECTED {exception?.Message}");
    }*/

    private class TextSerializer : INetworkSerializer
    {
        /*public object Deserialize(INetworkConnection connection, INetworkChannel? channel, ReadOnlyMemory<byte> data)
            => data.Span.ReadString(0, out _, out _);

        public void Serialize(INetworkConnection connection, INetworkChannel? channel, object message, IMemoryWriter<byte> writer)
            => writer.WriteString((string)message);*/

        public void Serialize(object message, IMemoryWriteable<byte> writer)
            => writer.WriteString((string)message);

        public object Deserialize(MemoryReader<byte> reader, ReadOnlySpan<byte> data)
            => reader.ReadString(data);
    }
}