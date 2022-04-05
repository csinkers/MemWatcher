using MemWatcher.Types;

namespace MemWatcher;

public interface IGhidraType
{
    string Namespace { get; }
    string Name { get; }
    bool IsFixedSize { get; }
    uint GetSize(History? history);
    History HistoryConstructor(string path, Func<string, string, string?> resolvePath);
    string? BuildPath(string accum, string relative);
    bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context);
    bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types); // Return true if any types were resolved
}