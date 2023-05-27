namespace GhidraData;

public class GEnum : IGhidraType
{
    readonly uint _size;
    public TypeKey Key { get; }
    public bool IsFixedSize => true;
    public Dictionary<uint, string> Elements { get; }
    public override string ToString() => Key.Name;

    public GEnum(TypeKey key, uint size, Dictionary<uint, string> elements)
    {
        Key = key;
        Elements = elements;
        _size = size;
    }

    public bool Unswizzle(Dictionary<TypeKey, IGhidraType> types) { return false; }
}