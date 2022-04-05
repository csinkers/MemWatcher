using System.Text;
using ImGuiNET;

namespace MemWatcher.Types;

public class GArray : IGhidraType
{
    class ArrayHistory : History
    {
        public ArrayHistory(string path, IGhidraType type, string[] elementPaths) : base(path, type)
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
    public History HistoryConstructor(string path, Func<string, string, string?> resolvePath)
    {
        var elemPaths = Enumerable.Range(0, (int)Count).Select(x => $"{path}/{x}").ToArray();
        return new ArrayHistory(path, this, elemPaths);
    }

    public string? BuildPath(string accum, string relative)
    {
        int dotIndex = relative.IndexOf('.');
        var part = dotIndex == -1 ? relative : relative[..dotIndex];
        var remainder = dotIndex == -1 ? "" : relative[(dotIndex + 1)..];

        if (!int.TryParse(part, out _)) 
            return null;

        accum += '/';
        accum += part;
        return remainder.Length == 0 ? accum : Type.BuildPath(accum, remainder);
    }

    public bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
        => Draw((ArrayHistory)history, address, buffer, previousBuffer, context);
    bool Draw(ArrayHistory history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        history.LastAddress = address;
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
            var elemHistory = context.History.GetOrCreateHistory(history.ElementPaths[i], Type);
            var color = Util.ColorForAge(context.Now - elemHistory.LastModifiedTicks);

            ImGui.TextColored(color, NumberLabels[i]);
            ImGui.SameLine();
            uint elemAddress = address + (uint)i * size;
            var slice = Util.SafeSlice(buffer, (uint)i * size, size);
            var oldSlice = Util.SafeSlice(previousBuffer, (uint)i * size, size);

            ImGui.PushID(i);
            if (openAll) ImGui.SetNextItemOpen(true);
            if (closeAll) ImGui.SetNextItemOpen(false);
            changed |= Type.Draw(elemHistory, elemAddress, slice, oldSlice, context);
            ImGui.PopID();
        }

        if (changed)
            history.LastModifiedTicks = context.Now;

        ImGui.TreePop();
        return changed;
    }
}