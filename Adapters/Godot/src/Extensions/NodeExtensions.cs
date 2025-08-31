namespace Markwardt;

public static class NodeExtensions
{
    public static void AddChildDeferred(this Node node, Node child, bool forceReadableName = true)
        => node.CallDeferred("add_child", child, forceReadableName);

    public static void RemoveChildDeferred(this Node node, Node child)
        => node.CallDeferred("remove_child", child);

    public static T WithName<T>(this T node, string name)
        where T : Node
        => node.Do(_ => node.Name = name);

    public static T WithParent<T>(this T node, Node parent, bool forceReadableName = true)
        where T : Node
        => node.Do(_ => parent.AddChildDeferred(node, forceReadableName));

    public static T WithScene<T>(this T node, INodeScene scene, bool forceReadableName = true)
        where T : Node
        => node.Do(_ => scene.Add(node, forceReadableName));

    public static void OnTreeEntered(this Node node, Action<CompositeDisposable> action)
    {
        CompositeDisposable? disposables = null;
        
        node.TreeEntered += () =>
        {
            disposables = new();
            action(disposables);
        };

        node.TreeExited += () => disposables?.Dispose();
    }
}