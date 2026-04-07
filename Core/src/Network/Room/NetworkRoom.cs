namespace Markwardt;

/*public interface INetworkRoom<TMessage>
{
    NetworkRoomState State { get; }
    string? Name { get; }
    string? Host { get; }
    IEnumerable<INetworkPort> Ports { get; }
    IEnumerable<string> Members { get; }

    void Start(string name, string? password = null);
    void SetPassword(string? password);
    void Join(string name, INetworkConnector<ReadOnlyMemory<byte>> connector, string? password = null);
    void Open(INetworkConnector<ReadOnlyMemory<byte>> connector);
    void Kick(string name, string? reason = null);
    void Send(string name, TMessage message);
    void Broadcast(TMessage message);
    void Leave();
}

public class NetworkRoom<TMessage>(INetworkRoomHandler<TMessage> handler) : INetworkRoom<TMessage>
{
    private Room? room;

    public NetworkRoomState State
    {
        get
        {
            if (room is null)
            {
                return NetworkRoomState.Offline;
            }
            else if (room is HostedRoom)
            {
                return NetworkRoomState.Host;
            }
            else if (room is JoinedRoom)
            {
                return NetworkRoomState.Joined;
            }
            else
            {
                throw new InvalidOperationException("Unknown room type");
            }
        }
    }

    public string? Name => room?.Name;
    public string? Host => room?.Host;
    public IEnumerable<INetworkPort> Ports => room?.Ports ?? [];
    public IEnumerable<string> Members => room?.Members ?? [];

    public void Start(string name, string? password = null)
    {
        if (State is not NetworkRoomState.Offline)
        {
            Leave();
        }

        room = new HostedRoom(this, name, password);
        TriggerStarted();
    }

    public void Join(string name, INetworkConnector<ReadOnlyMemory<byte>> connector, string? password = null)
    {
        if (State is not NetworkRoomState.Offline)
        {
            Leave();
        }

        room = new JoinedRoom(this, name, password, connector);
        TriggerJoining();
    }

    public void SetPassword(string? password)
        => AsHosted().Password = password;

    public void Open(INetworkConnector<ReadOnlyMemory<byte>> connector)
        => AsHosted().Open(connector);

    public void Send(string name, TMessage message)
        => AsRoom().Send(name, message);

    public void Broadcast(TMessage message)
        => AsRoom().Broadcast(message);

    public void Kick(string name, string? reason = null)
        => AsHosted().Kick(name, reason);

    public void Leave()
        => Leave(null);

    private void Leave(Exception? exception)
    {
        if (State is not NetworkRoomState.Offline)
        {
            room?.Dispose();
            room = null;
            TriggerLeft(exception);
        }
    }

    private Room AsRoom()
        => room ?? throw new InvalidOperationException("Not in a room");

    private HostedRoom AsHosted()
        => room as HostedRoom ?? throw new InvalidOperationException("Not hosting a room");

    private void TriggerStarted()
        => handler.OnStarted(this);

    private void TriggerJoining()
        => handler.OnJoining(this);

    private void TriggerJoined()
        => handler.OnJoined(this);

    private void TriggerPortOpened(INetworkPort port)
        => handler.OnPortOpened(this, port);

    private void TriggerPortClosed(INetworkPort port, Exception? exception)
        => handler.OnPortClosed(this, port, exception);

    private void TriggerMemberJoined(string name)
        => handler.OnMemberJoined(this, name);

    private void TriggerReceived(string sender, bool isBroadcast, TMessage message)
        => handler.OnReceived(this, sender, isBroadcast, message);

    private bool TriggerRouted(string sender, string? target, TMessage message)
        => handler.OnRouted(this, sender, target, message);

    private void TriggerMemberLeft(string name)
        => handler.OnMemberLeft(this, name);

    private void TriggerLeft(Exception? exception)
        => handler.OnLeft(this, exception);

    private sealed record RoomMessage
    {
        public KickMessage? Kick { get; set; }
        public JoinMessage? Join { get; set; }
        public AcceptJoinMessage? AcceptJoin { get; set; }
        public MemberJoinedMessage? MemberJoined { get; set; }
        public MemberLeftMessage? MemberLeft { get; set; }
        public ContentMessage? Content { get; set; }
    }

    private sealed record JoinMessage(string Name, string? Password);
    private sealed record KickMessage(string? Reason);
    private sealed record AcceptJoinMessage(string Host, List<string> Members);
    private sealed record MemberJoinedMessage(string Name);
    private sealed record MemberLeftMessage(string Name);
    private sealed record ContentMessage(string? Sender, string? Target, TMessage Content);

    private abstract class Room(NetworkRoom<TMessage> source, string name, string? password) : BaseDisposable, INetworkHandler<RoomMessage>
    {
        private readonly HashSet<INetworkConnectionOld<RoomMessage>> connections = [];

        protected NetworkRoom<TMessage> Source => source;

        public string Name => name;

        public virtual string? Host { get; }

        public string? Password { get; set; } = password;

        public abstract IEnumerable<string> Members { get; }

        private readonly HashSet<INetworkPort> ports = [];
        public IEnumerable<INetworkPort> Ports => ports;

        public abstract void Send(string name, TMessage message);
        public abstract void Broadcast(TMessage message);

        public void OnOpened(INetworkPort port)
        {
            ports.Add(port);
            source.TriggerPortOpened(port);
        }
        public void OnClosed(INetworkPort port, Exception? exception)
        {
            ports.Remove(port);
            source.TriggerPortClosed(port, exception);
        }

        public void OnConnecting(INetworkConnectionOld<RoomMessage> connection)
            => connections.Add(connection);

        public virtual void OnConnected(INetworkConnectionOld<RoomMessage> connection) { }

        public abstract void OnReceived(INetworkConnectionOld<RoomMessage> connection, RoomMessage message, IRecycler? recycler);

        public virtual void OnDisconnected(INetworkConnectionOld<RoomMessage> connection, Exception? exception)
            => connections.Remove(connection);

        protected void MultiSend(IEnumerable<INetworkConnectionOld<RoomMessage>> connections, RoomMessage message)
        {
            foreach (INetworkConnectionOld<RoomMessage> connection in connections)
            {
                connection.Send(message);
            }
        }

        protected override void OnDispose()
        {
            base.OnDispose();

            ports.DisposeAll();
            connections.DisposeAll();
        }
    }

    private sealed class HostedRoom(NetworkRoom<TMessage> source, string name, string? password) : Room(source, name, password)
    {
        private readonly string name = name;
        private readonly Dictionary<string, INetworkConnectionOld<RoomMessage>> connections = [];
        private readonly Dictionary<INetworkConnectionOld<RoomMessage>, string> connectionNames = [];

        public override IEnumerable<string> Members => connections.Keys;

        public void Open(INetworkConnector<ReadOnlyMemory<byte>> connector)
            => connector.AsConverted(new JsonConverter<RoomMessage>()).Open(this);

        public async void Kick(string name, string? reason = null)
        {
            if (connections.TryGetValue(name, out INetworkConnectionOld<RoomMessage>? connection))
            {
                connection.Send(new() { Kick = new(reason) });
                Remove(connection);
                await TimeSpan.FromSeconds(2).Delay();
                connection.Dispose();
            }
        }

        public override void Send(string name, TMessage message)
            => connections.GetValueOrDefault(name)?.Send(new() { Content = new(null, name, message) });

        public override void Broadcast(TMessage message)
            => MultiSend(connections.Values, new() { Content = new(null, null, message) });
        
        public override void OnReceived(INetworkConnectionOld<RoomMessage> connection, RoomMessage message, IRecycler? recycler)
        {
            if (message.Join is not null)
            {
                OnJoinMessage(connection, message.Join);
            }
            else if (message.Content is not null && connectionNames.TryGetValue(connection, out string? name))
            {
                OnContentMessage(connection, name, message.Content);
            }
        }

        public override void OnDisconnected(INetworkConnectionOld<RoomMessage> connection, Exception? exception)
        {
            base.OnDisconnected(connection, exception);
            
            Remove(connection);
        }

        private void Remove(INetworkConnectionOld<RoomMessage> connection)
        {
            if (connectionNames.Remove(connection, out string? name))
            {
                connections.Remove(name);
                Source.TriggerMemberLeft(name);
                MultiSend(connections.Values, new() { MemberLeft = new(name) });
            }
        }

        private void OnJoinMessage(INetworkConnectionOld<RoomMessage> connection, JoinMessage message)
        {
            if (message.Password != Password)
            {
                Kick(message.Name, "Incorrect password");
            }
            else if (!connections.TryAdd(message.Name, connection))
            {
                Kick(message.Name, "Name already taken");
            }
            else
            {
                connectionNames.Add(connection, message.Name);
                Source.TriggerMemberJoined(message.Name);

                if (connections.ContainsKey(message.Name))
                {
                    connection.Send(new() { AcceptJoin = new(name, connections.Keys.Where(x => x != message.Name).ToList()) });
                    MultiSend(connections.Values.Where(x => x != connection), new() { MemberJoined = new(message.Name) });
                }
            }
        }

        private void OnContentMessage(INetworkConnectionOld<RoomMessage> connection, string sender, ContentMessage message)
        {
            if (message.Target == Name)
            {
                Source.TriggerReceived(sender, false, message.Content);
            }
            else if (Source.TriggerRouted(sender, message.Target, message.Content))
            {
                if (message.Target is not null)
                {
                    connections.GetValueOrDefault(message.Target)?.Send(new() { Content = new(sender, message.Target, message.Content) });
                }
                else
                {
                    Source.TriggerReceived(sender, true, message.Content);
                    MultiSend(connections.Values.Where(x => x != connection), new() { Content = new(sender, null, message.Content) });
                }
            }
        }
    }

    private sealed class JoinedRoom : Room
    {
        public JoinedRoom(NetworkRoom<TMessage> source, string name, string? password, INetworkConnector<ReadOnlyMemory<byte>> connector)
            : base(source, name, password)
        {
            this.name = name;
            connector.AsConverted(new JsonConverter<RoomMessage>()).AsSingleConnection().Open(this);
        }

        private readonly string name;

        private RoomKickException? kickException;
        private INetworkConnectionOld<RoomMessage>? connection;

        public bool IsJoined { get; private set; }

        private readonly HashSet<string> members = [];
        public override IEnumerable<string> Members => members;

        public override void Send(string name, TMessage message)
            => connection?.Send(new() { Content = new(this.name, name, message) });

        public override void Broadcast(TMessage message)
            => connection?.Send(new() { Content = new(name, null, message) });

        public override void OnConnected(INetworkConnectionOld<RoomMessage> connection)
        {
            base.OnConnected(connection);

            this.connection = connection;
            connection.Send(new() { Join = new(Name, Password) });
        }

        public override void OnReceived(INetworkConnectionOld<RoomMessage> connection, RoomMessage message, IRecycler? recycler)
        {
            if (message.Kick is not null)
            {
                OnKickMessage(message.Kick);
            }
            else if (message.AcceptJoin is not null)
            {
                OnAcceptJoinMessage(message.AcceptJoin);
            }
            else if (message.MemberJoined is not null)
            {
                OnMemberJoinedMessage(message.MemberJoined);
            }
            else if (message.MemberLeft is not null)
            {
                OnMemberLeftMessage(message.MemberLeft);
            }
            else if (message.Content is not null)
            {
                OnContentMessage(message.Content);
            }
        }

        public override void OnDisconnected(INetworkConnectionOld<RoomMessage> connection, Exception? exception)
        {
            base.OnDisconnected(connection, exception);

            Source.Leave(kickException ?? exception);
        }

        private void OnKickMessage(KickMessage message)
        {
            kickException = new RoomKickException(message.Reason);
            connection?.Dispose();
        }

        private void OnAcceptJoinMessage(AcceptJoinMessage message)
        {
            IsJoined = true;
            members.UnionWith(message.Members);
            Source.TriggerJoined();
        }

        private void OnMemberJoinedMessage(MemberJoinedMessage message)
        {
            members.Add(message.Name);
            Source.TriggerMemberJoined(message.Name);
        }

        private void OnMemberLeftMessage(MemberLeftMessage message)
        {
            members.Remove(message.Name);
            Source.TriggerMemberLeft(message.Name);
        }

        private void OnContentMessage(ContentMessage message)
            => Source.TriggerReceived(message.Sender ?? Host.NotNull(), message.Target is null, message.Content);
    }
}*/