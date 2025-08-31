namespace Markwardt;

public interface ITextValidator
{
    Failable Validate(string text);
}