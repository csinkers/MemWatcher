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

    public uint Size => 0;
    public override string ToString() => $"Dummy({Namespace}, {Name})";
    public void Draw(string path, ReadOnlySpan<byte> buffer, SymbolLookup lookup) => throw new NotImplementedException();
    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) => throw new NotImplementedException();
}