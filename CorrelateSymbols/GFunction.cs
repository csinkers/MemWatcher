using System.Text.RegularExpressions;

namespace CorrelateSymbols;

public record GFunction(TypeKey Key, uint Address, uint MaxAddress)
{
    static readonly Regex DefaultName = new(@"^(FUN|LAB)_[0-9a-f]{8}$", RegexOptions.Compiled);

    public int Index { get; set; }
    public bool IsNamed => !DefaultName.IsMatch(Key.Name);
    public List<(uint Start, uint End)> Regions { get; } = new();
    public HashSet<GFunction> Callees { get; } = new();
    public HashSet<GFunction> Callers { get; } = new();
    public bool IsIgnored { get; set; }

    IEnumerable<int> Indices(IEnumerable<GFunction> functions)
    {
        int lastIndex = Index;
        foreach (var c in functions.OrderBy(x => x.Index))
        {
            yield return c.Index - lastIndex;
            lastIndex = c.Index;
        }
    }

    public IEnumerable<int> CalleeIndices => Indices(Callees);
    public IEnumerable<int> CallerIndices => Indices(Callers);

    public override string ToString()
    {
        var callees = string.Join(", ", CalleeIndices);
        var callers = string.Join(", ", CallerIndices);
        return $"{Callees.Count:D3} {Callers.Count:D3} {Key.Name}: {callees} | {callers}";
    }
}