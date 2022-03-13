using ImGuiNET;

namespace MemWatcher.Types;

public class GUnion : IGhidraType
{
    readonly uint _size;

    public GUnion(string ns, string name, uint size, List<GStructMember> members)
    {
        Namespace = ns;
        Name = name;
        Members = members ?? throw new ArgumentNullException(nameof(members));
        _size = size;
    }

    public string Namespace { get; }
    public string Name { get; }
    public bool IsFixedSize => true;
    public uint GetSize(History? history) => _size;
    public History HistoryConstructor() => History.DefaultConstructor();
    public List<GStructMember> Members { get; }

    public override string ToString() => $"union {Namespace}::{Name} ({_size:X})";
    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        foreach (var member in Members)
            member.Unswizzle(types);
    }

    public bool Draw(string path, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, long now, SymbolLookup lookup)
    {
        ImGui.Text("<UNION TODO>");
        return false;
    }
}