namespace Markwardt;

public interface IViewController
{
    Vector2 CameraRotation { get; set; }
    float CameraZoom { get; set; }
    Vector3 CameraPosition { get; set; }
    Node3D? CameraTarget { get; set; }

    float SunAmbientBalance { get; set; }

    Color AmbientColor { get; set; }
    float AmbientBrightness { get; set; }

    Vector2 SunRotation { get; set; }
    Color SunColor { get; set; }
    float SunBrightness { get; set; }
}

public class ViewController : NodeController, IViewController
{
    public ViewController(INodeScene scene)
        : base(scene)
    {
        Anchor.WithName("View");

        camera.WithParent(Anchor);
        camera.Environment = environment;

        sun.WithParent(Anchor);
        sun.ShadowEnabled = true;

        CameraRotation = new(45, 0);
        CameraZoom = 5;

        SunAmbientBalance = 0.75f;

        AmbientColor = Colors.White;
        AmbientBrightness = 1;

        SunColor = Colors.White;
        SunBrightness = 1;
    }

    private readonly Camera3D camera = new();
    private readonly DirectionalLight3D sun = new();

    private readonly Godot.Environment environment = new()
    {
        BackgroundMode = Godot.Environment.BGMode.Color,
        BackgroundColor = Colors.Black,
        AmbientLightSource = Godot.Environment.AmbientSource.Color
    };

    public Vector2 CameraRotation
    {
        get => new(-Anchor.RotationDegrees.X, Anchor.RotationDegrees.Y);
        set
        {
            RenderingServer.GlobalShaderParameterSet("camera_slant_rotation", value);
            Anchor.RotationDegrees = new(-value.X, value.Y, 0);
        }
    }

    public float CameraZoom
    {
        get => camera.Position.Z;
        set => camera.Position = new(0, 0, value);
    }

    public Vector3 CameraPosition
    {
        get => Anchor.Position;
        set
        {
            Anchor.Position = value;
            CameraTarget = null;
        }
    }

    public Node3D? CameraTarget { get; set; }

    private float sunAmbientBalance;
    public float SunAmbientBalance
    {
        get => sunAmbientBalance;
        set
        {
            sunAmbientBalance = value;
            UpdateLight();
        }
    }

    public Color AmbientColor
    {
        get => environment.AmbientLightColor;
        set => environment.AmbientLightColor = value;
    }

    private float ambientBrightness;
    public float AmbientBrightness
    {
        get => ambientBrightness;
        set
        {
            ambientBrightness = value;
            UpdateLight();
        }
    }

    public Vector2 SunRotation
    {
        get => new(-sun.RotationDegrees.X, sun.RotationDegrees.Y);
        set => sun.RotationDegrees = new(-value.X, value.Y, 0);
    }

    public Color SunColor
    {
        get => sun.LightColor;
        set => sun.LightColor = value;
    }

    private float sunBrightness;
    public float SunBrightness
    {
        get => sunBrightness;
        set
        {
            sunBrightness = value;
            UpdateLight();
        }
    }

    protected override void OnProcess(double delta)
    {
        if (CameraTarget is not null)
        {
            Anchor.Position = CameraTarget.Position;
        }
    }

    private void UpdateLight()
    {
        sun.LightEnergy = SunBrightness * SunAmbientBalance;
        environment.AmbientLightEnergy = AmbientBrightness * (1 - SunAmbientBalance);
    }
}