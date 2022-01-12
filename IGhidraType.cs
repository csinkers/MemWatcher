namespace MemWatcher;

public interface IGhidraType
{
    string Namespace { get; }
    string Name { get; }
    uint Size { get; }
    void Draw(string path, ReadOnlySpan<byte> buffer, SymbolLookup lookup);
    void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types);
}
