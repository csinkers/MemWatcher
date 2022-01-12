using ImGuiNET;

namespace MemWatcher;

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
    public uint Size => Constants.PointerSize;

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (ReturnType is GDummy dummy)
            ReturnType = types[(dummy.Namespace, dummy.Name)];

        foreach(var p in Parameters)
            p.Unswizzle(types);
    }

    public void Draw(string path, ReadOnlySpan<byte> buffer, SymbolLookup lookup) => ImGui.Text("func_ptr");
}