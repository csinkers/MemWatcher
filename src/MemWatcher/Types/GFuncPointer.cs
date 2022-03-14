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
    public History HistoryConstructor(string path) => History.DefaultConstructor(path);

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

    public bool Draw(History history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        if (buffer.IsEmpty)
        {
            ImGui.TextUnformatted("--");
            return false;
        }

        if (!previousBuffer.IsEmpty && !buffer.SequenceEqual(previousBuffer))
            history.LastModifiedTicks = context.Now;

        var color = Util.ColorForAge(context.Now - history.LastModifiedTicks);
        var address = MemoryMarshal.Read<uint>(buffer);
        ImGui.TextColored(color, context.Lookup.Describe(address)); // TODO: Ensure unformatted

        return history.LastModifiedTicks == context.Now;
    }
}