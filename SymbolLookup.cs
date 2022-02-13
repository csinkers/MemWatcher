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

    int FindNearest(uint address) // Binary search
    {
        int first = 0;
        int last = _symbols.Length - 1;
        int mid;

        do
        {
            mid = first + (last - first) / 2;
            if (address > _symbols[mid].Address)
                first = mid + 1;
            else
                last = mid - 1;

            if (_symbols[mid].Address == address)
                return mid;
        } while (first <= last);

        if (_symbols[mid].Address > address && mid != 0)
            mid--;

        return mid;
    }
}