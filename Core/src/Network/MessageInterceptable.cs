namespace Markwardt;

public interface IMessageInterceptable
{
    IEnumerable<IMessageInterceptor> Interceptors { get; }
}