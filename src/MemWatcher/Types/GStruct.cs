using ImGuiNET;

namespace MemWatcher.Types;

public class GStruct : IGhidraType
{
    class StructHistory : History
    {
        public StructHistory(string path, string[] memberPaths) : base(path)
            => MemberPaths = memberPaths ?? throw new ArgumentNullException(nameof(memberPaths));
        public string[] MemberPaths { get; }
        public override string ToString() => $"StructH:{Path}:{Util.Timestamp(LastModifiedTicks):g3}";
    }
    public string Namespace { get; }
    public string Name { get; }
    public uint Size { get; private set; }
    public bool IsFixedSize { get; }
    public string[] MemberNames { get; }
    public uint GetSize(History? history) => Size;
    public History HistoryConstructor(string path)
    {
        var memberPaths = Members.Select((x, i) => $"{path}/{i}").ToArray();
        return new StructHistory(path, memberPaths);
    }

    public List<GStructMember> Members { get; }

    public GStruct(string ns, string name, uint size, List<GStructMember> members)
    {
        Namespace = ns;
        Name = name;
        Members = members ?? throw new ArgumentNullException(nameof(members));
        Size = size;
        IsFixedSize = true; // In case the line below results in a recursive call back to this type
        IsFixedSize = Members.All(x => x.Type.IsFixedSize);
        MemberNames = Members.Select(x => $"[{x.Name}]".Replace("%", "%%")).ToArray();
    }

    public override string ToString() => $"struct {Namespace}::{Name} ({Size:X})";
    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        bool changed = false;
        foreach (var member in Members)
            changed |= member.Unswizzle(types);
        return changed;
    }

    public bool Draw(History history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
        => Draw((StructHistory)history, buffer, previousBuffer, context);
    bool Draw(StructHistory history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        bool changed = false;

        if (!ImGui.TreeNode(Name))
        {
            changed = !previousBuffer.IsEmpty && !buffer.SequenceEqual(previousBuffer);
            if (changed)
                history.LastModifiedTicks = context.Now;
            return changed;
        }

        uint size = 0;
        for (var i = 0; i < Members.Count; i++)
        {
            var member = Members[i];
            var memberPath = history.MemberPaths[i];
            var memberHistory = context.History.GetHistory(memberPath, member.Type);
            var color = Util.ColorForAge(context.Now - memberHistory.LastModifiedTicks);
            ImGui.TextColored(color, MemberNames[i]);
            ImGui.SameLine();

            if (!IsFixedSize)
                size += member.Type.GetSize(memberHistory);

            var slice = Util.SafeSlice(buffer, member.Offset, member.Size);
            var oldSlice = Util.SafeSlice(previousBuffer, member.Offset, member.Size);

            ImGui.PushID(i);
            changed |= member.Type.Draw(memberHistory, slice, oldSlice, context);
            ImGui.PopID();
        }

        if (changed)
            history.LastModifiedTicks = context.Now;

        if (!IsFixedSize)
            Size = size;

        ImGui.TreePop();
        return changed;
    }
}