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
    public List<GStructMember> Members { get; }
    public bool IsFixedSize => true;
    public uint GetSize(History? history) => _size;
    public History HistoryConstructor(string path, Func<string, string, string?> resolvePath) => History.DefaultConstructor(path, this);
    public string? BuildPath(string accum, string relative) => null; // TODO


    public override string ToString() => $"union {Namespace}::{Name} ({_size:X})";
    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        bool changed = false;
        foreach (var member in Members)
            changed |= member.Unswizzle(types);
        return changed;
    }

    public bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        history.LastAddress = address;
        ImGui.TextUnformatted("<UNION TODO>");
        return false;
    }
}