using MemWatcher.Types;

namespace MemWatcher;

public interface IGhidraType
{
    string Namespace { get; }
    string Name { get; }
    bool IsFixedSize { get; }
    uint GetSize(History? history);
    History HistoryConstructor();
    bool Draw(string path, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, long now, SymbolLookup lookup);
    void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types);
}
