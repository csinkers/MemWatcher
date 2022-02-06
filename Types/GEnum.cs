using ImGuiNET;

namespace MemWatcher.Types;

public class GEnum : IGhidraType
{
    public string Namespace { get; }
    public string Name { get; }
    public uint Size { get; }
    public Dictionary<uint, string> Elements { get; }
    public override string ToString() => Name;

    public GEnum(string ns, string name, uint size, Dictionary<uint, string> elements)
    {
        Name = name;
        Namespace = ns;
        Size = size;
        Elements = elements;
    }

    public void Draw(string path, ReadOnlySpan<byte> buffer, SymbolLookup lookup)
    {
        if (buffer.Length < Size)
        {
            ImGui.Text("--");
            return;
        }

        uint value = Size switch
        {
            1 => buffer[0],
            2 => BitConverter.ToUInt16(buffer),
            4 => BitConverter.ToUInt32(buffer),
            _ => throw new InvalidOperationException($"Unsupported enum size {Size}")
        };

        ImGui.Text(Elements.TryGetValue(value, out var name) 
            ? $"{name} ({value})" 
            : value.ToString());
    }

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) { }
}