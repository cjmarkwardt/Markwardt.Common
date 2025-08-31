namespace Markwardt;

public interface INodeScene
{
    void Add(Node node, bool forceReadableName = true);
}

public class NodeScene(SceneTree scene) : INodeScene
{
    public void Add(Node node, bool forceReadableName = true)
        => scene.Root.AddChildDeferred(node, forceReadableName);
}