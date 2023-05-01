using System.Text;

namespace CorrelateSymbols;

public class GArray : IGhidraType
{
    static readonly List<string> NumberLabels = new();

    public bool IsFixedSize => true;
    public IGhidraType Type { get; private set; }
    public uint Count { get; }
    public override string ToString() => Name;

    public GArray(IGhidraType type, uint count)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Count = count;
        while (NumberLabels.Count < Count)
            NumberLabels.Add($"[{NumberLabels.Count}] ");
    }

    public bool Unswizzle(Dictionary<TypeKey, IGhidraType> types)
    {
        if (Type is not GDummy dummy) 
            return false;

        Type = types[dummy.Key];
        return true;
    }

    string Name
    {
        get
        {
            IGhidraType type = this;
            StringBuilder sb = new();
            while (type is GArray array)
            {
                sb.Append('[');
                sb.Append(array.Count);
                sb.Append(']');
                type = array.Type;
            }

            return type.Key.Name + sb;
        }
    }

    public TypeKey Key => new(Type.Key.Namespace, Name);
}