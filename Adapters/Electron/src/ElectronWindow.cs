namespace Markwardt;

public class ElectronWindow<T>(string executablePath, string frontendUrl) : IFrontendWindow<T>, IHostHandler<T>
{
    private static readonly IConnectionProtocol<T, ReadOnlyMemory<byte>> protocol;

    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    static ElectronWindow()
        => protocol = new ConnectionProtocol<T>().AsStandardMessages().AsJson(serializerOptions).AsBytes().WithLengthPrefixBuffer();

    private readonly CancellationTokenSource cancellation = new();

    private bool isDisposed;
    private IDisposable? host = null;
    private IConnection<T>? connection;
    private Process? process;

    private readonly ReplaySubject<Exception?> closed = new();
    public IObservable<Exception?> Closed => closed;

    private readonly Subject<string> output = new();
    public IObservable<string> Output => output;

    private readonly BufferSubject<Packet> received = new();
    public IObservable<Packet> Received => received;

    public FrontendWindowState State { get; private set; }

    public async ValueTask Open(FrontendWindowOpenOptions? options = null)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);

        if (State is FrontendWindowState.Unopened)
        {
            State = FrontendWindowState.Opened;

            options ??= new();
            host = protocol.HostTcp(out int port, IPAddress.Loopback).Handle(this);
            FrontendConfiguration config = new(frontendUrl, port, options);

            process = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executablePath,
                    Arguments = '"' + JsonSerializer.Serialize(config, serializerOptions).Replace("\"", "\\\"") + '"',
                    WorkingDirectory = Path.GetDirectoryName(executablePath),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            process.StartInfo.Environment["ELECTRON_RUN_AS_NODE"] = null;

            void Output(string? data)
            {
                if (!string.IsNullOrEmpty(data))
                {
                    output.OnNext(data);
                }
            }

            process.OutputDataReceived += (sender, e) => Output(e.Data);

            string crashFlag = "<~%FRONT-CRASH%~>";
            string exitFlag = "<~%FRONT-EXIT%~>";
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    if (e.Data.StartsWith(crashFlag))
                    {
                        Close(new FrontendException(e.Data[crashFlag.Length..].Replace("<~%N%~>", "\n")));
                    }
                    else if (e.Data.StartsWith(exitFlag))
                    {
                        Close();
                    }
                    else
                    {
                        Output(e.Data);
                    }
                }
            };

            process.Exited += (sender, e) => Close();

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
    }

    public void Send(Packet packet)
        => connection?.Send(packet);

    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;

            Close();
            cancellation.Dispose();
        }
    }

    void IConnectionHandler<T>.OnConnected(IConnection<T> connection)
    {
        if (this.connection is null)
        {
            this.connection = connection;
        }
        else
        {
            connection.Dispose();
        }
    }

    void IConnectionHandler<T>.OnReceived(IConnection<T> connection, Packet<T> packet)
        => received.OnNext(packet.Value);

    void IConnectionHandler<T>.OnSignalReceived(IConnection<T> connection, Packet packet) { }

    void IConnectionHandler<T>.OnDisconnected(IConnection<T> connection, Exception? exception)
    {
        if (this.connection == connection)
        {
            Close(exception);
        }
    }

    void IHostHandler<T>.OnHostStopped(Exception? exception)
        => Close(exception);

    private void Close(Exception? exception = null)
    {
        if (State is not FrontendWindowState.Closed)
        {
            if (State is FrontendWindowState.Opened)
            {
                State = FrontendWindowState.Closed;

                cancellation.Cancel();
                host?.Dispose();
                connection?.Dispose();
                process?.Kill();
                process?.Dispose();

                closed.OnNext(exception);
                closed.OnCompleted();
            }
            else
            {
                State = FrontendWindowState.Closed;
            }
        }
    }

    private sealed record FrontendConfiguration(string Location, int Port, FrontendWindowOpenOptions OpenOptions);
}