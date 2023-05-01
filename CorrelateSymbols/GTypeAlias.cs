namespace CorrelateSymbols;

public class GTypeAlias : IGhidraType
{
    public TypeKey Key { get; }
    public IGhidraType Type { get; private set; }
    public bool IsFixedSize => Type.IsFixedSize;
    public override string ToString() => $"{Key.Name} = {Type}";

    public GTypeAlias(TypeKey key, IGhidraType type)
    {
        Key = key;
        Type = type;
    }

    public bool Unswizzle(Dictionary<TypeKey, IGhidraType> types)
    {
        if (Type is not GDummy dummy)
            return false;

        Type = types[dummy.Key];
        return true;
    }
}