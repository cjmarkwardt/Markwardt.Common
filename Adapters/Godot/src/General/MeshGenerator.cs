namespace Markwardt;

public readonly record struct Triangle2(Vector2 A, Vector2 B, Vector2 C)
{
    public static Triangle2 operator +(Triangle2 triangle, Vector2 vector)
        => new(triangle.A + vector, triangle.B + vector, triangle.C + vector);

    public static Triangle2 operator *(Triangle2 triangle, Vector2 vector)
        => new(triangle.A * vector, triangle.B * vector, triangle.C * vector);

    public readonly Triangle2 Flipped => new(C, B, A);
    public readonly Quad2 Quad => new(A, B, C, A + (C - B));
}

public readonly record struct Quad2(Vector2 A, Vector2 B, Vector2 C, Vector2 D)
{
    public static implicit operator Quad2(Rect2 rect)
        => new(rect.Position, new(rect.End.X, rect.Position.Y), rect.End, new(rect.Position.X, rect.End.Y));

    public static Quad2 operator +(Quad2 quad, Vector2 vector)
        => new(quad.A + vector, quad.B + vector, quad.C + vector, quad.D + vector);

    public static Quad2 operator *(Quad2 quad, Vector2 vector)
        => new(quad.A * vector, quad.B * vector, quad.C * vector, quad.D * vector);

    public readonly Quad2 Flipped => new(D, C, B, A);
    public readonly Triangle2 Triangle1 => new(A, B, C);
    public readonly Triangle2 Triangle2 => new(A, C, D);
}

public readonly record struct Triangle3(Vector3 A, Vector3 B, Vector3 C)
{
    public static Triangle3 operator +(Triangle3 triangle, Vector3 vector)
        => new(triangle.A + vector, triangle.B + vector, triangle.C + vector);

    public static Triangle3 operator *(Triangle3 triangle, Vector3 vector)
        => new(triangle.A * vector, triangle.B * vector, triangle.C * vector);

    public readonly Triangle3 Flipped => new(C, B, A);
    public readonly Quad3 Quad => new(A, B, C, A + (C - B));
}

public readonly record struct Quad3(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
{
    public static Quad3 operator +(Quad3 quad, Vector3 vector)
        => new(quad.A + vector, quad.B + vector, quad.C + vector, quad.D + vector);

    public static Quad3 operator *(Quad3 quad, Vector3 vector)
        => new(quad.A * vector, quad.B * vector, quad.C * vector, quad.D * vector);

    public readonly Quad3 Flipped => new(D, C, B, A);
    public readonly Triangle3 Triangle1 => new(A, B, C);
    public readonly Triangle3 Triangle2 => new(A, C, D);
}

public interface IMeshGenerator
{
    Vector3 Anchor { get; set; }
    Vector3 Scale { get; set; }

    void AddTriangle(Triangle3 triangle, Triangle2 texture);
    void AddQuad(Quad3 quad, Quad2 texture);
    Mesh Generate();
}

public class MeshGenerator : IMeshGenerator, IDisposable
{
    public MeshGenerator()
        => surface.Begin(Mesh.PrimitiveType.Triangles);

    private readonly SurfaceTool surface = new();

    public Vector3 Anchor { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;

    public void AddTriangle(Triangle3 triangle, Triangle2 texture)
    {
        surface.SetUV(texture.A);
        surface.AddVertex((Anchor + triangle.A) * Scale);
        surface.SetUV(texture.B);
        surface.AddVertex((Anchor + triangle.B) * Scale);
        surface.SetUV(texture.C);
        surface.AddVertex((Anchor + triangle.C) * Scale);
    }

    public void AddQuad(Quad3 quad, Quad2 texture)
    {
        AddTriangle(quad.Triangle1, texture.Triangle1);
        AddTriangle(quad.Triangle2, texture.Triangle2);
    }

    public Mesh Generate()
    {
        surface.Index();
        surface.GenerateNormals();
        surface.GenerateTangents();
        return surface.Commit();
    }

    public void Dispose()
        => surface.Dispose();
}