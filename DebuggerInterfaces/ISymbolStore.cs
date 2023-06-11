namespace DebuggerInterfaces;

public interface ISymbolStore
{
    int CodeOffset { get; set; }
    int DataOffset { get; set; }
    SymbolInfo? Lookup(uint address);
}