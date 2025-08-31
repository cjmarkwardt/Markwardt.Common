namespace Markwardt;

public partial class SpriteMaterial : ShaderMaterial
{
    public SpriteMaterial()
        => Shader = GD.Load<Shader>("res://Shaders/GameSprite.gdshader");

    public void SetAtlas(Texture2D atlas, int imageCount)
    {
        SetShaderParameter("Atlas", atlas);
        SetShaderParameter("ImageCount", imageCount);
    }

    public void ClearAtlas()
    {
        SetShaderParameter("ImageCount", 0);
        SetShaderParameter("Atlas", default);
    }

    public void SetImage(int? image)
        => SetShaderParameter("Image", image ?? -1);

    public void SetColors(Texture2D map)
    {
        Image image = Image.CreateEmpty(4, 1, false, Image.Format.Rgba8);
        image.SetPixel(0, 0, Colors.Red);
        image.SetPixel(1, 0, Colors.Red);
        image.SetPixel(2, 0, Colors.Red);
        image.SetPixel(3, 0, Colors.Red);
        ((ImageTexture)map).GetImage().SavePng($"D:\\Projects2\\Hammerlance.Oyster\\imagetest.png");
        SetShaderParameter("Colors", map);
    }
    //    => SetShaderParameter("Colors", map);

    public void SetFlipX(bool flip)
        => SetShaderParameter("FlipX", flip);

    public void SetFlipY(bool flip)
        => SetShaderParameter("FlipY", flip);
}