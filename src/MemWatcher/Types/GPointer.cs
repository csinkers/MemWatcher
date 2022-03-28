using ImGuiNET;

namespace MemWatcher.Types;

public class GPointer : IGhidraType
{
    class PointerHistory : History
    {
        public PointerHistory(string path) : base(path) { }
        public string? ReferentPath { get; set; }
        public override string ToString() => $"PtrH:{Path}:{Util.Timestamp(LastModifiedTicks):g3}";
    }

    public GPointer(IGhidraType type) => Type = type ?? throw new ArgumentNullException(nameof(type));
    public string Namespace => Type.Namespace;
    public string Name => $"{Type.Name} *";
    public IGhidraType Type { get; private set; }
    public bool IsFixedSize => true;
    public uint GetSize(History? history) => Constants.PointerSize;
    public History HistoryConstructor(string path) => new PointerHistory(path);
    public override string ToString() => Name;

    public bool Draw(History history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
        => Draw((PointerHistory)history, buffer, previousBuffer, context);

    bool Draw(PointerHistory history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        if (buffer.Length < Constants.PointerSize)
        {
            ImGui.TextUnformatted("--");
            return false;
        }

        history.ReferentPath ??= history.Path + "*";

        if (!previousBuffer.IsEmpty && !buffer.SequenceEqual(previousBuffer))
            history.LastModifiedTicks = context.Now;

        var color = Util.ColorForAge(context.Now - history.LastModifiedTicks);
        var address = BitConverter.ToUInt32(buffer);
        ImGui.TextColored(color, context.Lookup.Describe(address)); // TODO: Ensure unformatted
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
            var slice = context.Memory.Read(address, size);
            var oldSlice = context.Memory.ReadPrevious(address, size);

            ImGui.SetNextItemOpen(true);
            if (Type.Draw(referentHistory, slice, oldSlice, context))
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