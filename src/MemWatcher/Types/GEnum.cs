using ImGuiNET;

namespace MemWatcher.Types;

public class GEnum : IGhidraType
{
    readonly uint _size;

    public string Namespace { get; }
    public string Name { get; }
    public bool IsFixedSize => true;
    public uint GetSize(History? history) => _size;
    public History HistoryConstructor() => History.DefaultConstructor();
    public Dictionary<uint, string> Elements { get; }
    public override string ToString() => Name;

    public GEnum(string ns, string name, uint size, Dictionary<uint, string> elements)
    {
        Name = name;
        Namespace = ns;
        Elements = elements;
        _size = size;
    }

    public bool Draw(string path, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, long now, SymbolLookup lookup)
    {
        if (buffer.Length < _size)
        {
            ImGui.Text("--");
            return false;
        }

        var history = lookup.GetHistory(path, this);
        if (!buffer.SequenceEqual(previousBuffer))
            history.LastModifiedTicks = now;

        uint value = _size switch
        {
            1 => buffer[0],
            2 => BitConverter.ToUInt16(buffer),
            4 => BitConverter.ToUInt32(buffer),
            _ => throw new InvalidOperationException($"Unsupported enum size {_size}")
        };

        var color = Util.ColorForAge(now - history.LastModifiedTicks);
        ImGui.TextColored(color, Elements.TryGetValue(value, out var name) 
            ? $"{name} ({value})" 
            : value.ToString());

        return history.LastModifiedTicks == now;
    }

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) { }
}