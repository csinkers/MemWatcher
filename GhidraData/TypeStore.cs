namespace GhidraData;

public class TypeStore
{
    readonly Dictionary<TypeKey, IGhidraType> _types = new();
    public List<string> Errors { get; } = new();
    public List<TypeKey> AllKeys => _types.Keys.ToList();

    public void Add(TypeKey key, IGhidraType type)
    {
        _types[key] = type;
    }

    public void Add(IGhidraType type)
    {
        _types[type.Key] = type;
    }

    public IGhidraType this[TypeKey key] => Get(key);
    public IGhidraType Get(TypeKey key)
    {
        if (_types.TryGetValue(key, out var type))
            return type;

        Errors.Add($"Could not resolve \"{key}\"");
        return new GDummy(key);
    }
}