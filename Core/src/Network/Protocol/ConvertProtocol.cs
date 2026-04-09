namespace Markwardt.Network;

public class ConvertProtocol<TSend, TReceive>(IConverter<TSend, TReceive> converter) : IConnectionProtocol<TSend, TReceive>
{
    public ConvertProtocol(Func<TSend, TReceive> convert, Func<TReceive, TSend> revert)
        : this(new Converter<TSend, TReceive>(convert, revert)) { }

    public IConnectionProcessor<TSend, TReceive> CreateProcessor()
        => new Processor(converter);

    private sealed class Processor(IConverter<TSend, TReceive> converter) : ConvertProcessor<TSend, TReceive>
    {
        protected override TReceive Convert(TSend content)
            => converter.Convert(content);

        protected override TSend Revert(TReceive content)
            => converter.Revert(content);
    }
}