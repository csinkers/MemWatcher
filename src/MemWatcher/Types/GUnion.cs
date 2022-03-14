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
    public History HistoryConstructor(string path) => History.DefaultConstructor(path);
    public List<GStructMember> Members { get; }

    public override string ToString() => $"union {Namespace}::{Name} ({_size:X})";
    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        bool changed = false;
        foreach (var member in Members)
            changed |= member.Unswizzle(types);
        return changed;
    }

    public bool Draw(History history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        ImGui.TextUnformatted("<UNION TODO>");
        return false;
    }
}