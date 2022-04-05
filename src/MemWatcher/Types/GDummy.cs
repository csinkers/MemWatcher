using ImGuiNET;

namespace MemWatcher.Types;

public class GDummy : IGhidraType
{
    public string Namespace { get; }
    public string Name { get; }

    public GDummy(string ns, string name)
    {
        Name = name;
        Namespace = ns;
    }

    public bool IsFixedSize => true;
    public History HistoryConstructor(string path, Func<string, string, string?> resolvePath) => History.DefaultConstructor(path, this);
    public string? BuildPath(string accum, string relative) => null;
    public uint GetSize(History? history) => 0;
    public override string ToString() => $"Dummy({Namespace}, {Name})";
    string? _label;

    public bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        _label ??= $"<DUMMY TYPE {Namespace}/{Name}>";
        ImGui.TextUnformatted(_label);
        return false;
    }

    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
        => throw new InvalidOperationException(
            "Unswizzle should never be called on a dummy type - the calling type should recognise " +
            "it is a dummy in its own Unswizzle call and use the type dictionary to resolve it.");
}