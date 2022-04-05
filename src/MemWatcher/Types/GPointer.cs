using ImGuiNET;

namespace MemWatcher.Types;

public class GPointer : IGhidraType
{
    class PointerHistory : History
    {
        public PointerHistory(string path, IGhidraType type) : base(path, type) { }
        public string? ReferentPath { get; set; }
        public override string ToString() => $"PtrH:{Path}:{Util.Timestamp(LastModifiedTicks):g3}";
    }

    public GPointer(IGhidraType type) => Type = type ?? throw new ArgumentNullException(nameof(type));
    public string Namespace => Type.Namespace;
    public string Name => $"{Type.Name} *";
    public IGhidraType Type { get; private set; }
    public bool IsFixedSize => true;
    public uint GetSize(History? history) => Constants.PointerSize;
    public History HistoryConstructor(string path, Func<string, string, string?> resolvePath) => new PointerHistory(path, this);

    public string? BuildPath(string accum, string relative)
    {
        accum += '*';
        return Type.BuildPath(accum, relative);
    }

    public override string ToString() => Name;

    public bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
        => Draw((PointerHistory)history, address, buffer, previousBuffer, context);

    bool Draw(PointerHistory history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        history.LastAddress = address;
        if (buffer.Length < Constants.PointerSize)
        {
            ImGui.TextUnformatted("--");
            return false;
        }

        history.ReferentPath ??= history.Path + "*";

        if (!previousBuffer.IsEmpty && !buffer.SequenceEqual(previousBuffer))
            history.LastModifiedTicks = context.Now;

        var color = Util.ColorForAge(context.Now - history.LastModifiedTicks);
        var targetAddress = BitConverter.ToUInt32(buffer);
        ImGui.TextColored(color, context.Lookup.Describe(targetAddress)); // TODO: Ensure unformatted
        ImGui.SameLine();

        if (ImGui.TreeNode(Name))
        {
            var referentHistory = context.History.GetOrCreateHistory(history.ReferentPath, Type);
            if (history.Directives != null)
            {
                referentHistory.Directives = history.Directives;
                history.Directives = null;
            }

            var size = Type.GetSize(referentHistory);
            var slice = context.Memory.Read(targetAddress, size);
            var oldSlice = context.Memory.ReadPrevious(targetAddress, size);

            ImGui.SetNextItemOpen(true);
            if (Type.Draw(referentHistory, targetAddress, slice, oldSlice, context))
                history.LastModifiedTicks = context.Now;

            ImGui.TreePop();
        }

        return history.LastModifiedTicks == context.Now;
    }

    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is not GDummy dummy)
            return false;
        
        Type = types[(dummy.Namespace, dummy.Name)];
        return true;
    }
}