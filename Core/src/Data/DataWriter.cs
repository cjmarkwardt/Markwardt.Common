namespace Markwardt;

public interface IDataWriter
{
    ValueTask WriteNull();
    ValueTask WriteInteger(BigInteger? value);
    ValueTask WriteNumber(float? value);
    ValueTask WritePreciseNumber(double? value);
    ValueTask WriteText(string? value);
    ValueTask WriteObject(BigInteger? type, BigInteger? reference);
    ValueTask WriteReference(BigInteger value);
    ValueTask WriteSequence();
    ValueTask WriteStop();
}

public static class DataWriterExtensions
{
    public static async ValueTask<bool> TryWriteValue(this IDataWriter writer, object? value)
    {
        switch (value)
        {
            case null:
                await writer.WriteNull();
                return true;
            case bool boolean:
                await writer.WriteInteger(boolean ? 1 : 0);
                return true;
            case BigInteger integer:
                await writer.WriteInteger(integer);
                return true;
            case byte integer:
                await writer.WriteInteger(integer);
                return true;
            case sbyte integer:
                await writer.WriteInteger(integer);
                return true;
            case short integer:
                await writer.WriteInteger(integer);
                return true;
            case ushort integer:
                await writer.WriteInteger(integer);
                return true;
            case int integer:
                await writer.WriteInteger(integer);
                return true;
            case uint integer:
                await writer.WriteInteger(integer);
                return true;
            case long integer:
                await writer.WriteInteger(integer);
                return true;
            case ulong integer:
                await writer.WriteInteger(integer);
                return true;
            case float number:
                await writer.WriteNumber(number);
                return true;
            case double number:
                await writer.WritePreciseNumber(number);
                return true;
            case decimal number:
                await writer.WritePreciseNumber((double)number);
                return true;
            case string text:
                await writer.WriteText(text);
                return true;
            default:
                return false;
        }
    }
}

public class DataWriter(IDataPartWriter writer) : IDataWriter
{
    public async ValueTask WriteNull()
        => await writer.WriteNull();

    public async ValueTask WriteInteger(BigInteger? value)
    {
        if (value is null)
        {
            await writer.WriteNull();
        }
        else
        {
            await writer.WriteInteger(value.Value);
        }
    }

    public async ValueTask WriteNumber(float? value)
    {
        if (value is null)
        {
            await writer.WriteNull();
        }
        else
        {
            await writer.WriteSingle(value.Value);
        }
    }

    public ValueTask WritePreciseNumber(double? value)
    {
        if (value is null)
        {
            return writer.WriteNull();
        }
        else
        {
            return writer.WriteDouble(value.Value);
        }
    }

    public ValueTask WriteText(string? value)
    {
        if (value is null)
        {
            return writer.WriteNull();
        }
        else
        {
            return writer.WriteString(value);
        }
    }

    public async ValueTask WriteObject(BigInteger? type, BigInteger? reference)
    {
        DataSignal signal;
        if (type is not null && reference is not null)
        {
            signal = DataSignal.TypedReferencedObject;
        }
        else if (type is not null)
        {
            signal = DataSignal.TypedObject;
        }
        else if (reference is not null)
        {
            signal = DataSignal.ReferencedObject;
        }
        else
        {
            signal = DataSignal.Object;
        }

        await WriteSignal(signal);

        if (type is not null)
        {
            await writer.WriteInteger(type.Value);
        }

        if (reference is not null)
        {
            await writer.WriteInteger(reference.Value);
        }
    }

    public async ValueTask WriteReference(BigInteger value)
    {
        await WriteSignal(DataSignal.Reference);
        await WriteInteger(value);
    }

    public async ValueTask WriteSequence()
        => await WriteSignal(DataSignal.Sequence);

    public async ValueTask WriteStop()
        => await WriteSignal(DataSignal.Stop);

    private async ValueTask WriteSignal(DataSignal signal)
        => await writer.WriteCode((int)signal);
}