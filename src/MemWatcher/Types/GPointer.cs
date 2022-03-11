using ImGuiNET;

namespace MemWatcher.Types;

public class GPointer : IGhidraType
{
    public GPointer(IGhidraType type) => Type = type ?? throw new ArgumentNullException(nameof(type));
    public string Namespace => Type.Namespace;
    public string Name => $"{Type.Name} *";
    public IGhidraType Type { get; private set; }
    public uint Size => Constants.PointerSize;
    public override string ToString() => Name;

    public void Draw(string path, ReadOnlySpan<byte> buffer, SymbolLookup lookup)
    {
        if (buffer.Length < Size)
        {
            ImGui.Text("--");
            return;
        }

        var address = BitConverter.ToUInt32(buffer);
        ImGui.Text(lookup.Describe(address));
    }

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is GDummy dummy)
            Type = types[(dummy.Namespace, dummy.Name)];
    }
}