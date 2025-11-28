namespace Markwardt;

public class NamedConstructorAttribute(string name) : Attribute
{
    public string Name => name;
}