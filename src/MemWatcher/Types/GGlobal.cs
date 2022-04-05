using ImGuiNET;
namespace MemWatcher.Types;

public class GGlobal : IGhidraType
{
    public GGlobal(uint address, uint size, IGhidraType type)
    {
        Address = address;
        Size = size;
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public string Namespace { get; set; } = "";
    public string Name { get; set; } = "";
    public uint Address { get; }
    public uint Size { get; }
    public IGhidraType Type { get; private set; }
    public bool IsFixedSize => true;
    public override string ToString() => $"{Type} @ {Address:X} ({Size:X})";
    public uint GetSize(History? history) => Size;

    public History HistoryConstructor(string path, Func<string, string, string?> resolvePath)
    {
        var history = Type.HistoryConstructor(path, resolvePath);
        history.LastAddress = Address;
        return history;
    }

    public string? BuildPath(string accum, string relative) => Type.BuildPath(accum, relative);

    public bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        // var history = context.History.GetOrCreateHistory(Name, Data.Type);
        // var active = IsActive;
        // ImGui.Checkbox(CheckboxId, ref active);
        // IsActive = active;
        // ImGui.SameLine();

        // var color = Util.ColorForAge(context.Now - history.LastModifiedTicks);
        // ImGui.TextColored(color, _label);
        // ImGui.SameLine();

        ImGui.PushID(Name);
        var cur = context.Memory.Read(Address, Size);
        var prev = context.Refreshed ? context.Memory.ReadPrevious(Address, Size) : ReadOnlySpan<byte>.Empty;
        bool result = Type.Draw(history, Address, cur, prev, context);
        ImGui.PopID();
        return result;
    }

    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is not GDummy dummy) 
            return false;

        Type = types[(dummy.Namespace, dummy.Name)];
        return true;
    }
}