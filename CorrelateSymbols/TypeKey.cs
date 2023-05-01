namespace CorrelateSymbols;

public readonly record struct TypeKey(string Namespace, string Name)
{
    public override string ToString() => $"{Namespace}/{Name}";
}