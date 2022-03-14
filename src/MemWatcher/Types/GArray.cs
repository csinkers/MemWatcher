using System.Text;
using ImGuiNET;

namespace MemWatcher.Types;

public class GArray : IGhidraType
{
    class ArrayHistory : History
    {
        public ArrayHistory(string path, string[] elementPaths) : base(path)
            => ElementPaths = elementPaths ?? throw new ArgumentNullException(nameof(elementPaths));

        public string[] ElementPaths { get; }
        public override string ToString() => $"ArrayH:{Path}:{Util.Timestamp(LastModifiedTicks):g3}";
    }

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

    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is not GDummy dummy) 
            return false;

        Type = types[(dummy.Namespace, dummy.Name)];
        return true;
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
    public History HistoryConstructor(string path)
    {
        var elemPaths = Enumerable.Range(0, (int)Count).Select(x => $"{path}/{x}").ToArray();
        return new ArrayHistory(path, elemPaths);
    }

    public bool Draw(History history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
        => Draw((ArrayHistory)history, buffer, previousBuffer, context);
    bool Draw(ArrayHistory history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        if (Count == 0)
        {
            ImGui.TextUnformatted("<EMPTY>");
            return false;
        }

        if (Type == GPrimitive.Char)
        {
            if (!previousBuffer.IsEmpty && !buffer.SequenceEqual(previousBuffer))
                history.LastModifiedTicks = context.Now;

            var str = Constants.Encoding.GetString(buffer);
            var color = Util.ColorForAge(context.Now - history.LastModifiedTicks);
            ImGui.TextColored(color, str.Replace("%", "%%"));
            return history.LastModifiedTicks == context.Now;
        }

        bool openAll = ImGui.Button("+"); ImGui.SameLine();
        bool closeAll = ImGui.Button("-"); ImGui.SameLine();

        if (openAll) ImGui.SetNextItemOpen(true);
        if (closeAll) ImGui.SetNextItemOpen(false);

        bool changed = false;

        if (!ImGui.TreeNode(Name))
        {
            changed = !previousBuffer.IsEmpty && !buffer.SequenceEqual(previousBuffer);
            if (changed)
                history.LastModifiedTicks = context.Now;

            if (closeAll)
                ImGui.TreePush(Name);
            else
                return changed;
        }

        var size = Type.GetSize(null);
        for (int i = 0; i < Count; i++)
        {
            var elemHistory = context.History.GetHistory(history.ElementPaths[i], Type);
            var color = Util.ColorForAge(context.Now - elemHistory.LastModifiedTicks);

            ImGui.TextColored(color, NumberLabels[i]);
            ImGui.SameLine();
            var slice = Util.SafeSlice(buffer, (uint)i * size, size);
            var oldSlice = Util.SafeSlice(previousBuffer, (uint)i * size, size);

            ImGui.PushID(i);
            if (openAll) ImGui.SetNextItemOpen(true);
            if (closeAll) ImGui.SetNextItemOpen(false);
            changed |= Type.Draw(elemHistory, slice, oldSlice, context);
            ImGui.PopID();
        }

        if (changed)
            history.LastModifiedTicks = context.Now;

        ImGui.TreePop();
        return changed;
    }
}