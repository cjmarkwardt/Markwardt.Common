namespace Markwardt.ElectronTest;

public record Message
{
    public TestRequest? TestRequest { get; set; }
    public TestResponse? TestResponse { get; set; }
}