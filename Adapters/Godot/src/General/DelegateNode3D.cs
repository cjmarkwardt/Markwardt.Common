namespace Markwardt;

public static class DelegateNode3DExtensions
{
    public static T WithProcess<T>(this T node, Action<double> process)
        where T : DelegateNode3D
        => node.Do(x => x.Process = process);
}

public partial class DelegateNode3D : Node3D
{
    public Action<double>? Process { get; set; }

    public override void _Process(double delta)
    {
        base._Process(delta);
        Process?.Invoke(delta);
    }
}