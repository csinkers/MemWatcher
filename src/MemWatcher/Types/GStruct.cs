using ImGuiNET;

namespace MemWatcher.Types;

public class GStruct : IGhidraType
{
    public string Namespace { get; }
    public string Name { get; }
    public uint Size { get; private set; }
    public bool IsFixedSize { get; }
    public uint GetSize(History? history) => Size;
    public History HistoryConstructor()
    {
        var histories = Members.Select(x => (x, (string?)null, x.Type.HistoryConstructor())).ToArray();
        return new StructHistory(histories);
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
    }

    public override string ToString() => $"struct {Namespace}::{Name} ({Size:X})";
    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        foreach (var member in Members)
            member.Unswizzle(types);
    }

    public bool Draw(string path, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, long now, SymbolLookup lookup)
    {
        if (!ImGui.TreeNode(Name))
            return !buffer.SequenceEqual(previousBuffer);

        var history = (StructHistory)lookup.GetHistory(path, this);

        uint size = 0;
        bool changed = false;
        for (var i = 0; i < history.MemberHistories.Length; i++)
        {
            var (member, memberPath, memberHistory) = history.MemberHistories[i];
            if (memberPath == null)
            {
                memberPath = $"{path}/{i}";
                history.MemberHistories[i] = (member, memberPath, memberHistory);
            }

            var color = Util.ColorForAge(now - memberHistory.LastModifiedTicks);
            ImGui.TextColored(color, $"[{member.Name}]");
            ImGui.SameLine();

            if (!IsFixedSize)
                size += member.Type.GetSize(memberHistory);

            var slice = Util.SafeSlice(buffer, member.Offset, member.Size);
            var oldSlice = Util.SafeSlice(previousBuffer, member.Offset, member.Size);

            ImGui.PushID(i);
            changed |= member.Type.Draw(memberPath, slice, oldSlice, now, lookup);
            ImGui.PopID();
        }

        if (!IsFixedSize)
            Size = size;

        ImGui.TreePop();
        return changed;
    }
}