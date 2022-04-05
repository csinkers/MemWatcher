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
    public History HistoryConstructor(string path, Func<string, string, string?> resolvePath) => History.DefaultConstructor(path, this);
    public string? BuildPath(string accum, string relative) => null;

    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        bool changed = false;
        if (ReturnType is GDummy dummy)
        {
            ReturnType = types[(dummy.Namespace, dummy.Name)];
            changed = true;
        }

        foreach(var p in Parameters)
            changed |= p.Unswizzle(types);
        return changed;
    }

    public bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        history.LastAddress = address;
        if (buffer.IsEmpty)
        {
            ImGui.TextUnformatted("--");
            return false;
        }

        if (!previousBuffer.IsEmpty && !buffer.SequenceEqual(previousBuffer))
            history.LastModifiedTicks = context.Now;

        var color = Util.ColorForAge(context.Now - history.LastModifiedTicks);
        var targetAddress = MemoryMarshal.Read<uint>(buffer);
        ImGui.TextColored(color, context.Lookup.Describe(targetAddress)); // TODO: Ensure unformatted

        return history.LastModifiedTicks == context.Now;
    }
}