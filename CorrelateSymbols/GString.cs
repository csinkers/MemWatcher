namespace CorrelateSymbols;

public class GString : IGhidraType
{
    GString() { }
    public static readonly GString Instance = new();
    public TypeKey Key => new("/", "string");
    public bool IsFixedSize => false;
    public bool Unswizzle(Dictionary<TypeKey, IGhidraType> types) { return false; }
}