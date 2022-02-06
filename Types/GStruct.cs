using ImGuiNET;

namespace MemWatcher.Types;

public class GStruct : IGhidraType
{
    public string Namespace { get; }
    public string Name { get; }
    public uint Size { get; }
    public List<GStructMember> Members { get; }

    public GStruct(string ns, string name, uint size, List<GStructMember> members)
    {
        Namespace = ns;
        Name = name;
        Size = size;
        Members = members ?? throw new ArgumentNullException(nameof(members));
    }

    public override string ToString() => $"struct {Namespace}::{Name} ({Size:X})";
    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        foreach (var member in Members)
            member.Unswizzle(types);
    }

    public void Draw(string path, ReadOnlySpan<byte> buffer, SymbolLookup lookup)
    {
        if (!ImGui.TreeNode(path, Name))
            return;

        int index = 0;
        foreach (var member in Members)
        {
            ImGui.Text($"[{member.Name}]");
            ImGui.SameLine();
            var slice = Util.SafeSlice(buffer, member.Offset, member.Size);
            member.Type.Draw($"{path}/{index}", slice, lookup);
            index++;
        }
        ImGui.TreePop();
    }
}