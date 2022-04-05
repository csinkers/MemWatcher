using ImGuiNET;

namespace MemWatcher.Types;

public class GStruct : IGhidraType
{
    class StructHistory : History
    {
        public StructHistory(string path, IGhidraType type, string[] memberPaths, IGhidraType[] memberTypes) : base(path, type)
        {
            MemberPaths = memberPaths ?? throw new ArgumentNullException(nameof(memberPaths));
            MemberTypes = memberTypes ?? throw new ArgumentNullException(nameof(memberTypes));
        }

        public string[] MemberPaths { get; }
        public IGhidraType[] MemberTypes { get; }

        public override string ToString() => $"StructH:{Path}:{Util.Timestamp(LastModifiedTicks):g3}";
    }

    public List<GStructMember> Members { get; }
    public string Namespace { get; }
    public string Name { get; }
    public uint Size { get; private set; }
    public bool IsFixedSize { get; }
    public string[] MemberNames { get; }
    public uint GetSize(History? history) => Size;
    public History HistoryConstructor(string path, Func<string, string, string?> resolvePath)
    {
        var memberPaths = Members.Select((_, i) => $"{path}/{i}").ToArray();
        var memberTypes = Members.Select(x => x.Type).ToArray();

        List<IDirective>? directives = null;
        foreach (var member in Members)
        {
            if (member.Directives == null) continue;
            directives ??= new List<IDirective>();
            directives.AddRange(member.Directives);
        }

        return new StructHistory(path, this, memberPaths, memberTypes) { Directives = directives };
    }

    public string? BuildPath(string accum, string relative)
    {
        int dotIndex = relative.IndexOf('.');
        var part = dotIndex == -1 ? relative : relative[..dotIndex];
        var remainder = dotIndex == -1 ? "" : relative[(dotIndex + 1)..];

        for (int i = 0; i < Members.Count; i++)
        {
            var member = Members[i];
            if (member.Name == part)
            {
                accum += '/';
                accum += i.ToString();
                return remainder.Length == 0 ? accum : member.Type.BuildPath(accum, remainder);
            }
        }

        return null;
    }

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

    public bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
        => Draw((StructHistory)history, address, buffer, previousBuffer, context);
    bool Draw(StructHistory history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        bool changed = false;
        history.LastAddress = address;

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
            GStructMember member = Members[i];
            string memberPath = history.MemberPaths[i];
            var memberHistory = context.History.TryGetHistory(memberPath) ?? InitialiseMemberHistory(i, history, context.History);
            var memberType = history.MemberTypes[i];

            var color = Util.ColorForAge(context.Now - memberHistory.LastModifiedTicks);
            ImGui.TextColored(color, MemberNames[i]);
            ImGui.SameLine();

            if (!IsFixedSize)
                size += memberType.GetSize(memberHistory);

            uint memberAddress = address + member.Offset;
            var slice = Util.SafeSlice(buffer, member.Offset, member.Size);
            var oldSlice = Util.SafeSlice(previousBuffer, member.Offset, member.Size);

            ImGui.PushID(i);
            changed |= memberType.Draw(memberHistory, memberAddress, slice, oldSlice, context);
            ImGui.PopID();
        }

        if (changed)
            history.LastModifiedTicks = context.Now;

        if (!IsFixedSize)
            Size = size;

        ImGui.TreePop();
        return changed;
    }

    History InitialiseMemberHistory(int index, StructHistory history, HistoryCache historyCache)
    {
        GStructMember member = Members[index];
        string memberPath = history.MemberPaths[index];
        List<IDirective>? memberDirectives = null;

        if (history.Directives != null)
        {
            foreach (var directive in history.Directives)
            {
                if (directive is not DTargetChild(var path, var childDirective) || path != member.Name) continue;
                if (childDirective is DTypeCast cast)
                {
                    history.MemberTypes[index] = cast.Type;
                    continue;
                }

                memberDirectives ??= new List<IDirective>();
                memberDirectives.Add(childDirective);
            }
        }

        var memberHistory = historyCache.CreateHistory(memberPath, history.MemberTypes[index]);
        if (memberDirectives != null)
            memberHistory.Directives = memberDirectives;
        return memberHistory;
    }
}