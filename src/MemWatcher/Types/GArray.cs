using System.Text;
using ImGuiNET;

namespace MemWatcher.Types;

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
    public uint GetSize(History? history) => Type.GetSize(null) * Count;
    public History HistoryConstructor() => new ArrayHistory(new string[Count]);

    public bool Draw(string path, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, long now, SymbolLookup lookup)
    {
        if (Count == 0)
        {
            ImGui.Text("<EMPTY>");
            return false;
        }

        if (Type == GPrimitive.Char)
        {
            var charHistory = lookup.GetHistory(path, this);
            if (!buffer.SequenceEqual(previousBuffer))
                charHistory.LastModifiedTicks = now;

            var str = Constants.Encoding.GetString(buffer);
            var color = Util.ColorForAge(now - charHistory.LastModifiedTicks);
            ImGui.TextColored(color, str);
            return charHistory.LastModifiedTicks == now;
        }

        bool openAll = ImGui.Button("+"); ImGui.SameLine();
        bool closeAll = ImGui.Button("-"); ImGui.SameLine();

        if (openAll) ImGui.SetNextItemOpen(true);
        if (closeAll) ImGui.SetNextItemOpen(false);

        if (!ImGui.TreeNode(Name))
        {
            if (closeAll)
                ImGui.TreePush(Name);
            else
                return false;
        }

        var history = (ArrayHistory)lookup.GetHistory(path, this);
        if (history.Paths[0] == null)
            for (uint i = 0; i < Count; i++)
                history.Paths[i] = $"{path}/{i}";

        bool changed = false;
        var size = Type.GetSize(null);
        for (int i = 0; i < Count; i++)
        {
            ImGui.Text(NumberLabels[i]);
            ImGui.SameLine();
            var slice = Util.SafeSlice(buffer, (uint)i * size, size);
            var oldSlice = Util.SafeSlice(previousBuffer, (uint)i * size, size);

            ImGui.PushID(i);
            if (openAll) ImGui.SetNextItemOpen(true);
            if (closeAll) ImGui.SetNextItemOpen(false);
            changed |= Type.Draw(history.Paths[i]!, slice, oldSlice, now, lookup);
            ImGui.PopID();
        }

        ImGui.TreePop();
        return changed;
    }
}