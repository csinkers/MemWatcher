using System.Text;
using ImGuiNET;

namespace MemWatcher;

public class GArray : IGhidraType
{
    public IGhidraType Type { get; private set; }
    public uint Count { get; }
    public override string ToString() => Name;

    public GArray(IGhidraType type, uint count)
    {
        Type = type ?? throw new ArgumentNullException(nameof(type));
        Count = count;
    }

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is GDummy dummy)
            Type = types[(dummy.Namespace, dummy.Name)];
    }

    public string Name
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

            return type.Name + sb;
        }
    }

    public string Namespace => Type.Namespace;
    public uint Size => Type.Size * Count;
    public void Draw(string path, ReadOnlySpan<byte> buffer, SymbolLookup lookup)
    {
        if (Type == GPrimitive.Char)
        {
            var str = Constants.Encoding.GetString(buffer);
            ImGui.Text(str);
            return;
        }

        if (!ImGui.TreeNode(path, Name))
            return;

        for (uint i = 0; i < Count; i++)
        {
            ImGui.Text($"[{i}] ");
            ImGui.SameLine();
            var slice = Util.SafeSlice(buffer, i * Type.Size, Type.Size);
            Type.Draw($"{path}/{i}", slice, lookup);
        }

        ImGui.TreePop();
    }
}