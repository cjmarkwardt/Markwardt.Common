namespace Markwardt;

[Inject<IScreenController>]
public interface IScreenClearer
{
    void Clear();
}