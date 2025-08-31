namespace Markwardt;

public class OwnedShape<TShape> : IDisposable
    where TShape : Shape3D, new()
{
    public OwnedShape(CollisionObject3D body, GodotObject owner)
    {
        this.body = body;
        ownerId = body.CreateShapeOwner(owner);
        Value = new();

        body.ShapeOwnerAddShape(ownerId, Value);
    }

    private readonly CollisionObject3D body;
    private readonly uint ownerId;

    private bool isDisposed;

    public TShape Value { get; }

    public Vector3 Offset
    {
        get => body.ShapeOwnerGetTransform(ownerId).Origin;
        set => body.ShapeOwnerSetTransform(ownerId, Transform3D.Identity.Translated(value));
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            isDisposed = true;
            body.RemoveShapeOwner(ownerId);
        }
    }
}