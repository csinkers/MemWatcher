namespace MemWatcher.Types;

public class GStructMember
{
    public string Name { get; }
    public IGhidraType Type { get; private set; }
    public uint Offset { get; }
    public uint Size { get; }
    public string? Comment { get; }

    public GStructMember(string name, IGhidraType type, uint offset, uint size, string? comment)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        Name = name;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Offset = offset;
        Size = size;
        Comment = comment;
    }

    public override string ToString() => $"{Offset:X}: {Type.Name} {Name} ({Size:X})";

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is GDummy dummy)
            Type = types[(dummy.Namespace, dummy.Name)];
    }
}