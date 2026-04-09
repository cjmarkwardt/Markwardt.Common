namespace Markwardt;

public class SandboxStarter : IStarter
{
    public async ValueTask Start(CancellationToken cancellation = default)
    {
        Console.WriteLine("Sandbox");
    }
}