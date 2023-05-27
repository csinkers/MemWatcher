namespace GhidraData;

public class GPointer : IGhidraType
{
    public GPointer(IGhidraType type) => Type = type ?? throw new ArgumentNullException(nameof(type));
    public TypeKey Key => Type.Key;
    public IGhidraType Type { get; private set; }
    public bool IsFixedSize => true;
    public override string ToString() => Key.Name + " *";

    public bool Unswizzle(Dictionary<TypeKey, IGhidraType> types)
    {
        if (Type is not GDummy dummy)
            return false;
        
        Type = types[dummy.Key];
        return true;
    }
}