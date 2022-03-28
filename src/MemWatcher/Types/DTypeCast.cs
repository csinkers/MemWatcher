namespace MemWatcher.Types;

public class DTypeCast : IDirective
{
    public DTypeCast(IGhidraType type) { Type = type; }
    public IGhidraType Type { get; private set; }
    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is not GDummy dummy)
            return false;
        
        Type = types[(dummy.Namespace, dummy.Name)];
        return true;
    }
}