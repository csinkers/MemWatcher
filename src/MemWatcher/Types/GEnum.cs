using ImGuiNET;

namespace MemWatcher.Types;

public class GEnum : IGhidraType
{
    readonly uint _size;

    public string Namespace { get; }
    public string Name { get; }
    public bool IsFixedSize => true;
    public uint GetSize(History? history) => _size;
    public History HistoryConstructor(string path, Func<string, string, string?> resolvePath) => History.DefaultConstructor(path, this);
    public Dictionary<uint, string> Elements { get; }
    public override string ToString() => Name;

    public GEnum(string ns, string name, uint size, Dictionary<uint, string> elements)
    {
        Name = name;
        Namespace = ns;
        Elements = elements;
        _size = size;
    }

    public bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        history.LastAddress = address;
        if (buffer.Length < _size)
        {
            ImGui.TextUnformatted("--");
            return false;
        }

        if (!previousBuffer.IsEmpty && !buffer.SequenceEqual(previousBuffer))
            history.LastModifiedTicks = context.Now;

        uint value = _size switch
        {
            1 => buffer[0],
            2 => BitConverter.ToUInt16(buffer),
            4 => BitConverter.ToUInt32(buffer),
            _ => throw new InvalidOperationException($"Unsupported enum size {_size}")
        };

        var color = Util.ColorForAge(context.Now - history.LastModifiedTicks);
        ImGui.TextColored(color, Elements.TryGetValue(value, out var name) 
            ? $"{name} ({value})" 
            : value.ToString());

        return history.LastModifiedTicks == context.Now;
    }

    public string? BuildPath(string accum, string relative) => null;
    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) { return false; }
}