namespace MemWatcher;

public class SymbolLookup
{
    readonly (uint, string)[] _symbols;
    public SymbolLookup((uint, string)[] symbols) => _symbols = symbols ?? throw new ArgumentNullException(nameof(symbols));

    public string Describe(uint address)
    {
        if (address == 0)
            return "(null)";

        var index = FindNearest(address);
        var delta = (int)(address - index);
        var sym = _symbols[index];
        return delta > 0 ? $"{sym.Item2}+0x{delta:X}" : sym.Item2;
    }

    int FindNearest(uint address)
    {
        int first = 0;
        int last = _symbols.Length - 1;
        int mid;
        do
        {
            mid = first + (last - first) / 2;
            if (address > _symbols[mid].Item1)
                first = mid + 1;
            else
                last = mid - 1;
            if (_symbols[mid].Item1 == address)
                return mid;
        } while (first <= last);
        return mid;
    }
}