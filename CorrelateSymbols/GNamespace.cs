namespace CorrelateSymbols;

public class GNamespace : IGhidraType
{
    readonly string _name;
    public GNamespace(string name) => _name = name;
    public TypeKey Key => new(Constants.SpecialNamespace, _name);
    public bool IsFixedSize => true;
    public List<IGhidraType> Members { get; } = new();

    public bool Unswizzle(Dictionary<TypeKey, IGhidraType> types) => false;

    public bool Sort() // Return true if empty
    {
        Members.Sort((x, y) => Comparer<string>.Default.Compare(x.Key.Name, y.Key.Name));
        foreach(var ns in Members.OfType<GNamespace>())
            ns.Sort();

        return Members.Count == 0;
    }

    public GNamespace GetOrAddNamespace(string part)
    {
        var existing = Members.OfType<GNamespace>().FirstOrDefault(x => x.Key.Name == part);
        if (existing != null)
            return existing;

        var ns = new GNamespace(part);
        Members.Add(ns);
        return ns;
    }
}