namespace Markwardt;

public class FrontendSenderTag : ConstructorTag<ConfigurableFrontendSender>;

public interface IFrontendSender
{
    void Send(object? message);
}