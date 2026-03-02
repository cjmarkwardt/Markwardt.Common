namespace Markwardt;

public record struct ServiceResolution(Type Tag, object? Value, string? Source, IService? Service);