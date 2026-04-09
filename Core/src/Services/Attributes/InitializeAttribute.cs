namespace Markwardt;

[AttributeUsage(AttributeTargets.Class)]
public class InitializeAttribute(Type initializer) : Attribute
{
    public Type Initializer => initializer;
}

[AttributeUsage(AttributeTargets.Class)]
public class InitializeAttribute<TInitializer> : InitializeAttribute
    where TInitializer : IAsyncInitializer
{
    public InitializeAttribute()
        : base(typeof(TInitializer)) { }
}