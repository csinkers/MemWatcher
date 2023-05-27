namespace GhidraData;

public interface IGhidraType
{
    TypeKey Key { get; }
    bool IsFixedSize { get; }
    bool Unswizzle(Dictionary<TypeKey, IGhidraType> types); // Return true if any types were resolved
}