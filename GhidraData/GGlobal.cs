namespace GhidraData;

public class GGlobal : IGhidraType
{
    public GGlobal(uint address, uint size, IGhidraType type)
    {
        Address = address;
        Size = size;
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public TypeKey Key { get; set; } = new("", "");
    public uint Address { get; }
    public uint Size { get; }
    public IGhidraType Type { get; private set; }
    public bool IsFixedSize => true;
    public override string ToString() => $"{Type} @ {Address:X} ({Size:X})";

    public bool Unswizzle(Dictionary<TypeKey, IGhidraType> types)
    {
        if (Type is not GDummy dummy) 
            return false;

        Type = types[dummy.Key];
        return true;
    }
}