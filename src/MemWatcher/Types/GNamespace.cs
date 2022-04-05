using ImGuiNET;

namespace MemWatcher.Types;

public class GNamespace : IGhidraType
{
    public class NamespaceHistory : History
    {
        public NamespaceHistory(string path, IGhidraType type, string[] memberPaths, string[] memberLabels) : base(path, type)
        {
            MemberPaths = memberPaths ?? throw new ArgumentNullException(nameof(memberPaths));
            MemberLabels = memberLabels ?? throw new ArgumentNullException(nameof(memberLabels));
        }

        public string[] MemberPaths { get; }
        public string[] MemberLabels { get; }
    }

    public GNamespace(string name) => Name = name;
    public string Namespace => Constants.SpecialNamespace;
    public string Name { get; }
    public bool IsFixedSize => true;
    public List<IGhidraType> Members { get; } = new();
    public uint GetSize(History? history) => 0;
    public History HistoryConstructor(string path, Func<string, string, string?> resolvePath) 
    {
        var memberPaths = Members.Select((_, i) => $"{path}/{i}").ToArray();
        var memberLabels = Members.Select(x => x.Name.Replace("%", "%%") + ": ").ToArray();
        return new NamespaceHistory(path, this, memberPaths, memberLabels);
    }

    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) => false;

    public string? BuildPath(string accum, string relative)
    {
        int slashIndex = relative.IndexOf('/');
        var part = slashIndex == -1 ? relative : relative[..slashIndex];
        var remainder = slashIndex == -1 ? "" : relative[(slashIndex + 1)..];

        for (int i = 0; i < Members.Count; i++)
        {
            var member = Members[i];
            if (member.Name != part)
                continue;

            if (member is GNamespace ns)
            {
                accum += '/';
                accum += i.ToString();
                return remainder.Length == 0 ? accum : ns.BuildPath(accum, remainder);
            }

            if (member is GGlobal g)
            {
                accum += '/';
                accum += i.ToString();
                return remainder.Length == 0 ? accum : g.Type.BuildPath(accum, remainder);
            }
        }

        return null;
    }

    public bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        var h = (NamespaceHistory)history;
        for (var index = 0; index < Members.Count; index++)
        {
            var member = Members[index];
            if (member is GGlobal g && !IsShown(g, false, context.Filter))
                continue;

            var memberPath = h.MemberPaths[index];
            var memberLabel = h.MemberLabels[index];
            var memberHistory = context.History.GetOrCreateHistory(memberPath, member);

            if (!ImGui.TreeNode(memberLabel))
                continue;

            ImGui.PushID(index);
            member.Draw(memberHistory, address, buffer, previousBuffer, context);
            ImGui.PopID();
            ImGui.TreePop();
        }

        return false;
    }

    bool IsShown(GGlobal g, bool onlyShowActive, string filter)
    {
        if (!onlyShowActive && string.IsNullOrEmpty(filter))
            return true;

        // if (onlyShowActive && watch.IsActive)
        //     return true;

        return !string.IsNullOrEmpty(filter) && g.Name.Contains(filter, StringComparison.OrdinalIgnoreCase);
    }

    public bool Sort() // Return true if empty
    {
        Members.Sort((x, y) => Comparer<string>.Default.Compare(x.Name, y.Name));
        foreach(var ns in Members.OfType<GNamespace>())
            ns.Sort();

        return Members.Count == 0;
    }

    public GNamespace GetOrAddNamespace(string part)
    {
        var existing = Members.OfType<GNamespace>().FirstOrDefault(x => x.Name == part);
        if (existing != null)
            return existing;

        var ns = new GNamespace(part);
        Members.Add(ns);
        return ns;
    }
}