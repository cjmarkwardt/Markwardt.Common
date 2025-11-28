namespace Markwardt;

public interface IDataReader
{
    ValueTask<object?> Read(CancellationToken cancellation = default);
}

public static class DataReaderExtensions
{
    public static async ValueTask<T> Read<T>(this IDataReader reader, CancellationToken cancellation = default)
        => (T)(await reader.Read(cancellation))!;

    public static async ValueTask ReadAllToString(this IDataReader reader, Stream output)
    {
        using StreamWriter writer = new(output, leaveOpen: true);

        bool isFirstLine = true;
        async ValueTask Write(string message)
        {
            if (isFirstLine)
            {
                isFirstLine = false;
            }
            else
            {
                await writer.WriteLineAsync();
            }

            await writer.WriteAsync(message);
        }

        while (true)
        {
            object? value;
            try
            {
                value = await reader.Read();
            }
            catch (EndOfDataException)
            {
                break;
            }

            switch (value)
            {
                case null:
                    await Write("null");
                    break;
                case BigInteger integer:
                    await Write(integer.ToString());
                    break;
                case float number:
                    await Write(number.ToString());
                    break;
                case double preciseNumber:
                    await Write(preciseNumber.ToString());
                    break;
                case string text:
                    await Write($"\"{text}\"");
                    break;
                case DataObjectSignal obj:
                    await Write($"T:{obj.TypeId}{(obj.Reference is not null ? $" R:{obj.Reference}" : string.Empty)}");
                    break;
                case DataReferenceSignal reference:
                    await Write($"R:{reference.Reference}");
                    break;
                case DataStopSignal:
                    await Write($"stop");
                    break;
                default:
                    throw new InvalidOperationException($"Unknown data type: {value.GetType()}");
            }
        }
    }

    public static async ValueTask<string> ReadAllToString(this IDataReader reader)
    {
        using MemoryStream memoryStream = new();
        await ReadAllToString(reader, memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);
        using StreamReader streamReader = new(memoryStream);
        return await streamReader.ReadToEndAsync();
    }
}

public class DataReader(IDataPartReader reader) : IDataReader
{
    public async ValueTask<object?> Read(CancellationToken cancellation = default)
    {
        object? value = await reader.Read(cancellation);
        if (value is DataCode code)
        {
            return (DataSignal)code.Value switch
            {
                DataSignal.Object => await ReadObject(false, cancellation),
                DataSignal.ReferencedObject => await ReadObject(true, cancellation),
                DataSignal.Reference => new DataReferenceSignal((int)await reader.Read<BigInteger>(cancellation)),
                DataSignal.Stop => new DataStopSignal(),
                _ => throw new NotSupportedException($"Signal {code.Value} unsupported.")
            };
        }
        else
        {
            return value;
        }
    }

    private async ValueTask<DataObjectSignal> ReadObject(bool isReferenced, CancellationToken cancellation = default)
        => new((int)await reader.Read<BigInteger>(cancellation), isReferenced ? (int)await reader.Read<BigInteger>(cancellation) : null);
}