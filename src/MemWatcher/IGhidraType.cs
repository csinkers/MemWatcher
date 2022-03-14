using MemWatcher.Types;

namespace MemWatcher;

public interface IGhidraType
{
    string Namespace { get; }
    string Name { get; }
    bool IsFixedSize { get; }
    uint GetSize(History? history);
    History HistoryConstructor(string path);
    bool Draw(History history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context);
    bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types);
}