namespace MemWatcher.Types;

public class GTypeAlias : IGhidraType
{
    public string Namespace { get; }
    public string Name { get; }
    public IGhidraType Type { get; private set; }
    public bool IsFixedSize => Type.IsFixedSize;
    public uint GetSize(History? history) => Type.GetSize(history);
    public History HistoryConstructor(string path, Func<string, string, string?> resolvePath) => History.DefaultConstructor(path, this);
    public string? BuildPath(string accum, string relative) => Type.BuildPath(accum, relative);
    public override string ToString() => $"{Name} = {Type}";

    public GTypeAlias(string ns, string name, IGhidraType type)
    {
        Namespace = ns;
        Name = name;
        Type = type;
    }

    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is not GDummy dummy)
            return false;

        Type = types[(dummy.Namespace, dummy.Name)];
        return true;
    }

    public bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
         => Type.Draw(history, address, buffer, previousBuffer, context);
}