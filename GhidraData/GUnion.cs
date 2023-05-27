namespace GhidraData;

public class GUnion : IGhidraType
{
    readonly uint _size;

    public GUnion(TypeKey key, uint size, List<GStructMember> members)
    {
        Key = key;
        Members = members ?? throw new ArgumentNullException(nameof(members));
        _size = size;
    }

    public TypeKey Key { get; }
    public List<GStructMember> Members { get; }
    public bool IsFixedSize => true;

    public override string ToString() => $"union {Key.Namespace}::{Key.Name} ({_size:X})";
    public bool Unswizzle(Dictionary<TypeKey, IGhidraType> types)
    {
        bool changed = false;
        foreach (var member in Members)
            changed |= member.Unswizzle(types);

        return changed;
    }
}