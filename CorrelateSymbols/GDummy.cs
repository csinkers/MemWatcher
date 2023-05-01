namespace CorrelateSymbols;

public class GDummy : IGhidraType
{
    public TypeKey Key { get; }
    public GDummy(TypeKey key) => Key = key;
    public bool IsFixedSize => true;
    public override string ToString() => $"Dummy({Key.Namespace}, {Key.Name})";
    public bool Unswizzle(Dictionary<TypeKey, IGhidraType> types)
        => throw new InvalidOperationException(
            "Unswizzle should never be called on a dummy type - the calling type should recognise " +
            "it is a dummy in its own Unswizzle call and use the type dictionary to resolve it.");
}