namespace MemWatcher;

public class SymbolLookup
{
    readonly (uint Address, string Name)[] _symbols;

    public SymbolLookup((uint, string)[] symbols) => _symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));

    public string Describe(uint address)
    {
        if (address == 0)
            return "(null)";

        var index = FindNearest(address);
        var symbol = _symbols[index];
        var delta = (int)(address - symbol.Address);
        var sign = delta < 0 ? '-' : '+';
        var absDelta = Math.Abs(delta);
        return delta > 0 
            ? $"{symbol.Name}{sign}0x{absDelta:X} ({address:X})" 
            : symbol.Name;
    }

    int FindNearest(uint address) => Util.FindNearest(_symbols, address);
}