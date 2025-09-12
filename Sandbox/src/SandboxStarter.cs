using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Markwardt;

public class SandboxStarter : IStarter
{
    public required IDataConfiguration Configuration { get; init; }
    public required IDataSaveSerializer Serializer { get; init; }

    public async ValueTask Start()
    {
        Serializer.Schema = "Test";
        Configuration.Configure(Serializer.Schema, new DataSerializerSource(new Dictionary<Type, IDataSerializer>()
        {
            { typeof(IThing), new DataObjectSerializer<IThing>() }
        }));

        IThing thing = DataObject.Create<IThing>();
        thing.Child.Chain(x => x.Labels.Changes).SelectMany(x => x).Subscribe(x => Console.WriteLine($"Labels changed: {x}"));
        thing.Id.Value = 5;
        thing.Id.Value = 6;
        thing.Name.Value = "Alan";
        thing.Child.Value = DataObject.Create<IThing>();
        thing.Child.Value.Labels.Add("x", 5);
        thing.Child.Value.Labels.Add("y", 8);
        thing.Child.Value.Labels.Remove("x");
        thing.Child.Value.Id.Value = 10;
        thing.Child.Value.Id.Value = 11;
        thing.Child.Value.Name.Value = "Child";
        thing.Child2.Value = thing.Child.Value;
        thing.Labels.Add("Blah", 5);
        thing.Labels.Add("Bloo", 10);
        thing.Children.Add("one", thing.Child.Value);
        thing.Children.Add("two", DataObject.Create<IThing>());
        thing.Children["two"].Labels.Add("x", 90);
        Console.WriteLine($"---- {thing.Child == thing.Child2}");

        using MemoryStream stream = new();
        await Serializer.Serialize(stream, thing);
        stream.Position = 0;
        Console.WriteLine(stream.Length);

        DataReader reader = new(new DataPartReader(new BlockReader(stream)));
        Console.WriteLine("-------------");
        Console.WriteLine(await reader.ReadAllToString());
        Console.WriteLine("-------------");

        stream.Position = 0;

        thing = (IThing)await Serializer.Deserialize(stream);
        Console.WriteLine(thing.Id);
        Console.WriteLine(thing.Name);
        Console.WriteLine(thing.Child.Value.Id.Value);
        Console.WriteLine(thing.Child.Value.Name);
        Console.WriteLine(thing.Child2.Value.Id);
        Console.WriteLine(thing.Child2.Value.Name);
        Console.WriteLine($"---- {thing.Child == thing.Child2}");
        Console.WriteLine(thing.Labels.Count);
        Console.WriteLine(thing.Labels["Blah"]);
        Console.WriteLine(thing.Labels["Bloo"]);
        Console.WriteLine(thing.Children.Count);
        Console.WriteLine(thing.Children["one"].Labels.Count);
        Console.WriteLine(thing.Children["two"].Labels["x"]);
    }

    public interface IThing : IDataObject
    {
        ISourceValue<byte> Id { get; }
        ISourceValue<string> Name { get; }

        ISourceValue<IThing> Child { get; }
        ISourceValue<IThing> Child2 { get; }

        ISourceDictionary<byte, string> Labels { get; }
        ISourceDictionary<IThing, string> Children { get; }
    }
}