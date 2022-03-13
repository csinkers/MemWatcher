namespace MemWatcher.Types;

public class GDummy : IGhidraType
{
    public string Namespace { get; }
    public string Name { get; }

    public GDummy(string ns, string name)
    {
        Name = name;
        Namespace = ns;
    }

    public bool IsFixedSize => true;
    public History HistoryConstructor() => History.DefaultConstructor();
    public uint GetSize(History? history) => 0;
    public override string ToString() => $"Dummy({Namespace}, {Name})";
    public bool Draw(string path, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, long now, SymbolLookup lookup)
        => throw new NotImplementedException();
    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) => throw new NotImplementedException();
}