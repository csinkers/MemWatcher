using ImGuiNET;

namespace MemWatcher.Types;

public class GPointer : IGhidraType
{
    public GPointer(IGhidraType type) => Type = type ?? throw new ArgumentNullException(nameof(type));
    public string Namespace => Type.Namespace;
    public string Name => $"{Type.Name} *";
    public IGhidraType Type { get; private set; }
    public bool IsFixedSize => true;
    public uint GetSize(History? history) => Constants.PointerSize;
    public History HistoryConstructor() => History.DefaultConstructor();
    public override string ToString() => Name;

    public bool Draw(string path, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, long now, SymbolLookup lookup)
    {
        if (buffer.Length < Constants.PointerSize)
        {
            ImGui.Text("--");
            return false;
        }

        var history = lookup.GetHistory(path, this);
        if (!buffer.SequenceEqual(previousBuffer))
            history.LastModifiedTicks = now;

        var color = Util.ColorForAge(now - history.LastModifiedTicks);
        var address = BitConverter.ToUInt32(buffer);
        ImGui.TextColored(color, lookup.Describe(address));
        return history.LastModifiedTicks == now;
    }

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is GDummy dummy)
            Type = types[(dummy.Namespace, dummy.Name)];
    }
}