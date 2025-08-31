namespace Markwardt;

public class SandboxStarter : IStarter
{
    public ValueTask Start()
    {
        Console.WriteLine("Sandbox");
        return ValueTask.CompletedTask;
    }
}