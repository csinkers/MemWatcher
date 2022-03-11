using ImGuiNET;

namespace MemWatcher.Types;

public class GUnion : IGhidraType
{
    public GUnion(string ns, string name, uint size, List<GStructMember> members)
    {
        Namespace = ns;
        Name = name;
        Size = size;
        Members = members ?? throw new ArgumentNullException(nameof(members));
    }

    public string Namespace { get; }
    public string Name { get; }
    public uint Size { get; }
    public List<GStructMember> Members { get; }

    public override string ToString() => $"union {Namespace}::{Name} ({Size:X})";
    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        foreach (var member in Members)
            member.Unswizzle(types);
    }

    public void Draw(string path, ReadOnlySpan<byte> buffer, SymbolLookup lookup)
    {
        ImGui.Text("<UNION TODO>");
    }
}