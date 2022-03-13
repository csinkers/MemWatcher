namespace MemWatcher.Types;

public class GTypeAlias : IGhidraType
{
    public string Namespace { get; }
    public string Name { get; }
    public bool IsFixedSize => Type.IsFixedSize;
    public uint GetSize(History? history) => Type.GetSize(history);
    public History HistoryConstructor() => History.DefaultConstructor();
    public IGhidraType Type { get; private set; }
    public override string ToString() => $"{Name} = {Type}";

    public GTypeAlias(string ns, string name, IGhidraType type)
    {
        Namespace = ns;
        Name = name;
        Type = type;
    }

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is GDummy dummy)
            Type = types[(dummy.Namespace, dummy.Name)];
    }

    public bool Draw(string path, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, long now, SymbolLookup lookup)
         => Type.Draw(path, buffer, previousBuffer, now, lookup);
}