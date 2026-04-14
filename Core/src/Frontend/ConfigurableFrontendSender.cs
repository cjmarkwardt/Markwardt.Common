namespace Markwardt;

public interface IConfigurableFrontendSender : IFrontendSender
{
    void Configure(IFrontendSender sender);
}

public class ConfigurableFrontendSender : IConfigurableFrontendSender
{
    private IFrontendSender? sender;

    public void Configure(IFrontendSender sender)
        => this.sender = sender;

    public void Send(object? message)
        => sender.NotNull("Frontend sender not configured").Send(message);
}