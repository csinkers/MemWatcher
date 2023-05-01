namespace CorrelateSymbols;

public class GStruct : IGhidraType
{
    public List<GStructMember> Members { get; }
    public TypeKey Key { get; }
    public uint Size { get; private set; }
    public bool IsFixedSize { get; }
    public string[] MemberNames { get; }

    public GStruct(TypeKey key, uint size, List<GStructMember> members)
    {
        Key = key;
        Members = members ?? throw new ArgumentNullException(nameof(members));
        Size = size;
        IsFixedSize = true; // In case the line below results in a recursive call back to this type
        IsFixedSize = Members.All(x => x.Type.IsFixedSize);
        MemberNames = Members.Select(x => $"[{x.Name}]".Replace("%", "%%")).ToArray();
    }

    public override string ToString() => $"struct {Key.Namespace}::{Key.Name} ({Size:X})";
    public bool Unswizzle(Dictionary<TypeKey, IGhidraType> types)
    {
        bool changed = false;
        foreach (var member in Members)
            changed |= member.Unswizzle(types);
        return changed;
    }
}