namespace MemWatcher.Types;

public class GFuncParameter
{
    public uint Ordinal { get; }
    public string Name { get; }
    public IGhidraType Type { get; private set; }
    public uint Size { get; }

    public GFuncParameter(uint ordinal, string name, uint size, IGhidraType type)
    {
        Ordinal = ordinal;
        Name = name;
        Type = type;
        Size = size;
    }

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is GDummy dummy)
            Type = types[(dummy.Namespace, dummy.Name)];
    }
}