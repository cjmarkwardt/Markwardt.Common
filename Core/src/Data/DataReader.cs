namespace Markwardt;

public class EndOfDataException() : Exception("No more data to read.");

public interface IDataReader
{
    ValueTask<object?> Read();
}

public static class DataReaderExtensions
{
    public static async ValueTask<T> Read<T>(this IDataReader reader, bool isNullable = false)
    {
        object? value = await reader.Read();
        if (value is null && !isNullable)
        {
            throw new InvalidOperationException();
        }

        return (T)value!;
    }

    public static async ValueTask ReadAllToString(this IDataReader reader, Stream output)
    {
        using StreamWriter writer = new(output, leaveOpen: true);

        int level = 0;
        async ValueTask Write(string message)
            => await writer.WriteLineAsync($"{new string('\t', level)}{message}");

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
                    await Write(text);
                    break;
                case DataObjectSignal obj:
                    await Write($"T:{obj.TypeId}{(obj.Reference is not null ? $" R:{obj.Reference}" : string.Empty)}");
                    level++;
                    break;
                case DataReferenceSignal reference:
                    await Write($"R:{reference.Reference}");
                    break;
                case DataSequenceSignal:
                    await Write($"...");
                    level++;
                    break;
                case DataStopSignal:
                    level--;

                    if (level == -1)
                    {
                        return;
                    }
                    else
                    {
                        break;
                    }
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
    public async ValueTask<object?> Read()
    {
        object? value = await reader.Read();
        if (value is DataCode code)
        {
            return (DataSignal)code.Value switch
            {
                DataSignal.Object => await ReadObject(false, false),
                DataSignal.TypedObject => await ReadObject(true, false),
                DataSignal.ReferencedObject => await ReadObject(false, true),
                DataSignal.TypedReferencedObject => await ReadObject(true, true),
                DataSignal.Reference => new DataReferenceSignal((int)await reader.Read<BigInteger>()),
                DataSignal.Sequence => new DataSequenceSignal(),
                DataSignal.Stop => new DataStopSignal(),
                _ => throw new NotSupportedException($"Signal {code.Value} unsupported.")
            };
        }
        else
        {
            return value;
        }
    }

    private async ValueTask<DataObjectSignal> ReadObject(bool isTyped, bool isReferenced)
        => new((int)await reader.Read<BigInteger>(), isReferenced ? (int)await reader.Read<BigInteger>() : null);
}