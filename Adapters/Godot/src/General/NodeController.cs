namespace Markwardt;

public abstract class NodeController
{
    public NodeController(INodeScene scene)
    {
        Anchor = new DelegateNode3D().WithProcess(OnProcess).WithScene(scene);
    }

    protected Node3D Anchor { get; }

    protected abstract void OnProcess(double delta);
}