namespace Markwardt;

public interface IDataReferenceSource
{
    int? Get(object value);
}