using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Markwardt;

public class SandboxStarter : IStarter
{
    public required IDataConfiguration Configuration { get; init; }
    public required IDataSaveSerializer Serializer { get; init; }

    public IEnumerable<string> Get<T, TValue>(Expression<Func<T, TValue>> selector)
    {
        List<string> properties = [];

        Expression? expression = selector.Body;
        while (expression is not null && expression is MemberExpression memberExpression)
        {
            properties.Add(memberExpression.Member.Name);
            expression = memberExpression.Expression;
        }

        properties.Reverse();
        return properties;
    }

    public async ValueTask Start()
    {
        Serializer.Schema = "Test";
        Configuration.Configure(Serializer.Schema, new DataSerializerSource(new Dictionary<Type, IDataSerializer>()
        {
            { typeof(IThing), new DataObjectSerializer<IThing>() }
        }));

        Console.WriteLine(string.Join(", ", Get((IThing x) => x)));
        Console.WriteLine(string.Join(", ", Get((IThing x) => x.Child)));
        Console.WriteLine(string.Join(", ", Get((IThing x) => x.Child.Id)));

        IThing thing = DataObject.Create<IThing>();
        thing.Watch(x => x.Child).Select(x => x.WatchItems(y => y.Labels)).Merge().Subscribe(x => Console.WriteLine($"Labels changed: {x}"));
        thing.Id = 5;
        thing.Id = 6;
        thing.Name = "Alan";
        thing.Child = DataObject.Create<IThing>();
        thing.Child.Labels.Add("x", 5);
        thing.Child.Labels.Add("y", 8);
        thing.Child.Labels.Remove("x");
        thing.Child.Id = 10;
        thing.Child.Id = 11;
        thing.Child.Name = "Child";
        thing.Child2 = thing.Child;
        thing.Labels.Add("Blah", 5);
        thing.Labels.Add("Bloo", 10);
        thing.Children.Add("one", thing.Child);
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
        Console.WriteLine(thing.Child.Id);
        Console.WriteLine(thing.Child.Name);
        Console.WriteLine(thing.Child2.Id);
        Console.WriteLine(thing.Child2.Name);
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
        byte Id { get; set; }
        string Name { get; set; }

        IThing Child { get; set; }
        IThing Child2 { get; set; }

        IDictionary<string, byte> Labels { get; }
        IDictionary<string, IThing> Children { get; }
    }
}