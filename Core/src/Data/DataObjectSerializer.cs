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
                if (propertyValue is IObservableDictionary dictionary)
                {
                    foreach (KeyValuePair<object?, object?> item in dictionary.Items)
                    {
                        context.Collect(item.Key);
                        context.Collect(item.Value);
                    }
                }
                else if (propertyValue is IObservableCollection collection)
                {
                    foreach (object? item in collection.Items)
                    {
                        context.Collect(item);
                    }
                }
                else if (propertyValue is IObservableValue observableValue)
                {
                    context.Collect(observableValue.Value);
                }
            }
        }
    }

    public override async ValueTask Serialize(IDataSerializationContext context, IDataWriter writer, IDataObject value)
    {
        foreach (DataObjectProperty property in layout.Properties.Values)
        {
            if (value.DataProperties.TryGetValue(property.Name, out object? propertyValue))
            {
                await writer.WriteInteger(property.Index);

                if (propertyValue is IObservableDictionary dictionary)
                {
                    await writer.WriteSequence();

                    foreach ((object? key, object? item) in dictionary.Items)
                    {
                        if (collection.KeyType is not null)
                        {
                            await context.Serialize(writer, key);
                        }

                        await context.Serialize(writer, item);
                    }

                    await writer.WriteStop();
                }
                else if (propertyValue is IObservableValue observableValue)
                {
                    await context.Serialize(writer, observableValue.Value);
                }
                else
                {
                    await context.Serialize(writer, propertyValue);
                }
            }
        }

        await writer.WriteStop();
    }

    public override async ValueTask<IDataObject> Deserialize(IDataDeserializationContext context, IDataReader reader)
    {
        DataObject obj = new(type);
        while (true)
        {
            object? value = await reader.Read();
            if (value is BigInteger index)
            {
                DataObjectProperty property = layout.IndexedProperties[(int)index];
                if (property.CollectionType is not null)
                {
                    value = await DeserializeCollection(context, reader, property.CollectionType);
                }
                else
                {
                    value = await context.Deserialize(reader);
                }

                obj.DataProperties[property.Name] = Convert(value, property.Type).NotNull();
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

    private async ValueTask SerializeCollection(IDataSerializationContext context, IDataWriter writer, )
    {
        await writer.WriteSequence();

        foreach ((object? key, object? item) in collection.Items)
        {
            if (collection.KeyType is not null)
            {
                await context.Serialize(writer, key);
            }

            await context.Serialize(writer, item);
        }

        await writer.WriteStop();
    }

    private async ValueTask<IDataCollection> DeserializeCollection(IDataDeserializationContext context, IDataReader reader, DataCollectionType collectionType)
    {
        await reader.Read<DataSequenceSignal>();

        IDataCollection collection = collectionType.Create();
        while (true)
        {
            object? key = null;

            object? item = await context.Deserialize(reader);
            if (item is DataStopSignal)
            {
                break;
            }

            if (collection.KeyType is not null)
            {
                key = Convert(item, collection.KeyType);
                item = Convert(await context.Deserialize(reader), collection.ItemType);
            }
            else
            {
                item = Convert(item, collection.ItemType);
            }

            collection.Inject(key, item);
        }

        return collection;
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

    private sealed class Property(PropertyInfo source, int index)
    {
        public int Index => index;
        public string Name => source.Name;
        public Type Type => source.PropertyType;
    }
}

public class DataObjectSerializer<T>() : DataObjectSerializer(typeof(T))
    where T : class, IDataObject;