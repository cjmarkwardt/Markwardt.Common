namespace Markwardt;

public class DataObjectSerializer(Type type) : DataSerializer<IDataObject>
{
    private readonly DataObjectLayout layout = DataObjectLayout.Get(type);

    public override bool IsCollectible => true;

    public override void Collect(IDataSerializationContext context, IDataObject value)
    {
        foreach (DataObjectProperty property in layout.Properties.Values)
        {
            if (value.DataProperties.TryGetValue(property.Name, out object? propertyValue))
            {
                if (propertyValue is IObservableDictionary targetDictionary)
                {
                    foreach (KeyValuePair<object?, object?> item in targetDictionary.Items)
                    {
                        context.Collect(item.Key);
                        context.Collect(item.Value);
                    }
                }
                else if (propertyValue is IObservableCollection targetCollection)
                {
                    foreach (object? item in targetCollection.Items)
                    {
                        context.Collect(item);
                    }
                }
                else if (propertyValue is IObservableValue targetValue)
                {
                    context.Collect(targetValue.Value);
                }
            }
        }
    }

    public override async ValueTask Serialize(IDataSerializationContext context, IDataWriter writer, IDataObject value, CancellationToken cancellation = default)
    {
        foreach (DataObjectProperty property in layout.Properties.Values)
        {
            if (value.DataProperties.TryGetValue(property.Name, out object? propertyValue))
            {
                if (IsEmpty(propertyValue))
                {
                    continue;
                }

                await writer.WriteInteger(property.Index);

                if (propertyValue is IObservableDictionary targetDictionary)
                {
                    foreach ((object? key, object? item) in targetDictionary.Items)
                    {
                        await context.Serialize(writer, key);
                        await context.Serialize(writer, item);
                    }

                    await writer.WriteStop();
                }
                else if (propertyValue is IObservableCollection targetCollection)
                {
                    foreach (object? item in targetCollection.Items)
                    {
                        await context.Serialize(writer, item);
                    }

                    await writer.WriteStop();
                }
                else if (propertyValue is IObservableValue targetValue)
                {
                    await context.Serialize(writer, targetValue.Value);
                }
            }
        }

        await writer.WriteStop();
    }

    public override async ValueTask<IDataObject> Deserialize(IDataDeserializationContext context, IDataReader reader, CancellationToken cancellation = default)
    {
        DataObject obj = new(type);
        while (true)
        {
            object? value = await reader.Read();
            if (value is BigInteger index)
            {
                DataObjectProperty property = layout.IndexedProperties[(int)index];
                value = obj.DataProperties[property.Name];
                if (property.IsDictionary)
                {
                    ISourceDictionary targetDictionary = (ISourceDictionary)value;
                    while (true)
                    {
                        object? key = await context.Deserialize(reader);
                        if (key is DataStopSignal)
                        {
                            break;
                        }

                        targetDictionary.SetKey(Convert(key, targetDictionary.KeyType), Convert(await context.Deserialize(reader), targetDictionary.ValueType));
                    }
                }
                else if (property.IsCollection)
                {
                    ISourceCollection targetCollection = (ISourceCollection)value;
                    while (true)
                    {
                        object? item = await context.Deserialize(reader);
                        if (item is DataStopSignal)
                        {
                            break;
                        }

                        targetCollection.Add(Convert(item, targetCollection.ItemType));
                    }
                }
                else if (property.IsValue)
                {
                    ISourceValue targetValue = (ISourceValue)value;
                    targetValue.Value = Convert(await context.Deserialize(reader), targetValue.ValueType);
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            else if (value is DataStopSignal)
            {
                return Impromptu.DynamicActLike(obj, layout.DataType, typeof(IDataObject));
            }
            else
            {
                throw new InvalidOperationException("Expected property index or stop signal");
            }
        }
    }

    private object? Convert(object? value, Type targetType)
    {
        if (value is null || value.GetType().IsAssignableTo(targetType))
        {
            return value;
        }
        else if (value is BigInteger integer)
        {
            if (targetType == typeof(bool))
            {
                return integer != 0;
            }
            else if (targetType == typeof(int))
            {
                return (int)integer;
            }
            else if (targetType == typeof(long))
            {
                return (long)integer;
            }
            else if (targetType == typeof(byte))
            {
                return (byte)integer;
            }
            else if (targetType == typeof(sbyte))
            {
                return (sbyte)integer;
            }
            else if (targetType == typeof(short))
            {
                return (short)integer;
            }
            else if (targetType == typeof(ushort))
            {
                return (ushort)integer;
            }
            else if (targetType == typeof(uint))
            {
                return (uint)integer;
            }
            else if (targetType == typeof(ulong))
            {
                return (ulong)integer;
            }
            else if (targetType == typeof(BigInteger))
            {
                return integer;
            }
        }

        throw new InvalidOperationException($"Cannot convert {value.GetType()} to {targetType}");
    }

    private bool IsEmpty(object? value)
        => (value is IObservableValue targetValue && targetValue.Value is null) || (value is IObservableCollection targetCollection && targetCollection.Count == 0);

    private sealed class Property(PropertyInfo source, int index)
    {
        public int Index => index;
        public string Name => source.Name;
        public Type Type => source.PropertyType;
    }
}

public class DataObjectSerializer<T>() : DataObjectSerializer(typeof(T))
    where T : class, IDataObject;