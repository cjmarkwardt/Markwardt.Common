namespace Markwardt;

public partial class SimpleSurface : MeshInstance3D
{
    public SimpleSurface()
    {
        mesh = new QuadMesh();
        material = new StandardMaterial3D();

        Mesh = mesh;
        mesh.Orientation = PlaneMesh.OrientationEnum.Y;
        mesh.Material = material;

        material.Uv1Triplanar = true;
    }

    private readonly QuadMesh mesh;
    private readonly StandardMaterial3D material;

    public Color Color
    {
        get => material.AlbedoColor;
        set => material.AlbedoColor = value;
    }

    public Texture2D? Texture
    {
        get => material.AlbedoTexture;
        set => material.AlbedoTexture = value;
    }

    public Vector2 Size
    {
        get => mesh.Size;
        set => mesh.Size = value;
    }
}