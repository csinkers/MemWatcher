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
        var nearestAddress = _symbols[index].Item1;
        var delta = (int)(address - nearestAddress);
        var sign = delta < 0 ? '-' : '+';
        var absDelta = Math.Abs(delta);
        var sym = _symbols[index];
        return delta > 0 ? $"{sym.Item2}{sign}0x{absDelta:X} ({address:X})" : sym.Item2;
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

        if (mid < address && mid != 0)
            mid--;
        return mid;
    }
}