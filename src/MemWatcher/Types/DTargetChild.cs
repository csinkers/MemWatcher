namespace MemWatcher.Types;

public record DTargetChild(string Path, IDirective Directive) : IDirective
{
    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) => false;
}