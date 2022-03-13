using System.Runtime.InteropServices;
using ImGuiNET;

namespace MemWatcher.Types;

public class GFuncPointer : IGhidraType
{
    public GFuncPointer(string ns, string name, IGhidraType returnType, List<GFuncParameter> parameters)
    {
        Namespace = ns;
        Name = name;
        ReturnType = returnType;
        Parameters = parameters;
    }

    public string Namespace { get; }
    public string Name { get; }
    public IGhidraType ReturnType { get; private set; }
    public List<GFuncParameter> Parameters { get; }
    public bool IsFixedSize => true;
    public uint GetSize(History? history) => Constants.PointerSize;
    public History HistoryConstructor() => History.DefaultConstructor();

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (ReturnType is GDummy dummy)
            ReturnType = types[(dummy.Namespace, dummy.Name)];

        foreach(var p in Parameters)
            p.Unswizzle(types);
    }

    public bool Draw(string path, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, long now, SymbolLookup lookup)
    {
        var history = lookup.GetHistory(path, this);
        if (!buffer.SequenceEqual(previousBuffer))
            history.LastModifiedTicks = now;

        var color = Util.ColorForAge(now - history.LastModifiedTicks);
        var address = MemoryMarshal.Read<uint>(buffer);
        ImGui.TextColored(color, lookup.Describe(address));

        return history.LastModifiedTicks == now;
    }
}