namespace Markwardt;

public interface IDataWriter
{
    ValueTask WriteNull(CancellationToken cancellation = default);
    ValueTask WriteInteger(BigInteger? value, CancellationToken cancellation = default);
    ValueTask WriteNumber(float? value, CancellationToken cancellation = default);
    ValueTask WritePreciseNumber(double? value, CancellationToken cancellation = default);
    ValueTask WriteText(string? value, CancellationToken cancellation = default);
    ValueTask WriteObject(BigInteger? type, BigInteger? reference, CancellationToken cancellation = default);
    ValueTask WriteReference(BigInteger value, CancellationToken cancellation = default);
    ValueTask WriteStop(CancellationToken cancellation = default);
}

public static class DataWriterExtensions
{
    public static async ValueTask<bool> TryWriteValue(this IDataWriter writer, object? value, CancellationToken cancellation = default)
    {
        switch (value)
        {
            case null:
                await writer.WriteNull(cancellation);
                return true;
            case bool boolean:
                await writer.WriteInteger(boolean ? 1 : 0, cancellation);
                return true;
            case BigInteger integer:
                await writer.WriteInteger(integer, cancellation);
                return true;
            case byte integer:
                await writer.WriteInteger(integer, cancellation);
                return true;
            case sbyte integer:
                await writer.WriteInteger(integer, cancellation);
                return true;
            case short integer:
                await writer.WriteInteger(integer, cancellation);
                return true;
            case ushort integer:
                await writer.WriteInteger(integer, cancellation);
                return true;
            case int integer:
                await writer.WriteInteger(integer, cancellation);
                return true;
            case uint integer:
                await writer.WriteInteger(integer, cancellation);
                return true;
            case long integer:
                await writer.WriteInteger(integer, cancellation);
                return true;
            case ulong integer:
                await writer.WriteInteger(integer, cancellation);
                return true;
            case float number:
                await writer.WriteNumber(number, cancellation);
                return true;
            case double number:
                await writer.WritePreciseNumber(number, cancellation);
                return true;
            case decimal number:
                await writer.WritePreciseNumber((double)number, cancellation);
                return true;
            case string text:
                await writer.WriteText(text, cancellation);
                return true;
            default:
                return false;
        }
    }
}

public class DataWriter(IDataPartWriter writer) : IDataWriter
{
    public async ValueTask WriteNull(CancellationToken cancellation = default)
        => await writer.WriteNull(cancellation);

    public async ValueTask WriteInteger(BigInteger? value, CancellationToken cancellation = default)
    {
        if (value is null)
        {
            await writer.WriteNull(cancellation);
        }
        else
        {
            await writer.WriteInteger(value.Value, cancellation);
        }
    }

    public async ValueTask WriteNumber(float? value, CancellationToken cancellation = default)
    {
        if (value is null)
        {
            await writer.WriteNull(cancellation);
        }
        else
        {
            await writer.WriteSingle(value.Value, cancellation);
        }
    }

    public ValueTask WritePreciseNumber(double? value, CancellationToken cancellation = default)
    {
        if (value is null)
        {
            return writer.WriteNull(cancellation);
        }
        else
        {
            return writer.WriteDouble(value.Value, cancellation);
        }
    }

    public ValueTask WriteText(string? value, CancellationToken cancellation = default)
    {
        if (value is null)
        {
            return writer.WriteNull(cancellation);
        }
        else
        {
            return writer.WriteString(value, cancellation);
        }
    }

    public async ValueTask WriteObject(BigInteger? type, BigInteger? reference, CancellationToken cancellation = default)
    {
        DataSignal signal;
        if (reference is not null)
        {
            signal = DataSignal.ReferencedObject;
        }
        else
        {
            signal = DataSignal.Object;
        }

        await WriteSignal(signal, cancellation);

        if (type is not null)
        {
            await writer.WriteInteger(type.Value, cancellation);
        }

        if (reference is not null)
        {
            await writer.WriteInteger(reference.Value, cancellation);
        }
    }

    public async ValueTask WriteReference(BigInteger value, CancellationToken cancellation = default)
    {
        await WriteSignal(DataSignal.Reference, cancellation);
        await WriteInteger(value, cancellation);
    }

    public async ValueTask WriteStop(CancellationToken cancellation = default)
        => await WriteSignal(DataSignal.Stop, cancellation);

    private async ValueTask WriteSignal(DataSignal signal, CancellationToken cancellation = default)
        => await writer.WriteCode((int)signal, cancellation);
}