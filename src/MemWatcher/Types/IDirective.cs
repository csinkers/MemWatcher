namespace MemWatcher.Types;

public interface IDirective
{
    bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types); // Return true if any types were resolved
}