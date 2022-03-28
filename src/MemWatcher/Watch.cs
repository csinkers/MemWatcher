using ImGuiNET;
using MemWatcher.Types;

namespace MemWatcher;

public class Watch
{
    public Watch(string name, GData data)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Data = data ?? throw new ArgumentNullException(nameof(data));
        // CheckboxId = Data.Address.ToString("X8");
        _label = Name.Replace("%", "%%") + ": ";
    }

    // string CheckboxId { get; }
    public string Name { get; }
    public GData Data { get; }
    // public bool IsActive { get; set; }
    public long LastChangeTimeTicks { get; set; }
    // public override string ToString() => $"[{(IsActive ? 'x' : ' ')}] {Name}: {Data}";
    public override string ToString() => $"{Name}: {Data}";
    readonly string _label;

    public void Draw(DrawContext context)
    {
        // var active = IsActive;
        // ImGui.Checkbox(CheckboxId, ref active);
        // IsActive = active;
        // ImGui.SameLine();

        var color = Util.ColorForAge(context.Now - LastChangeTimeTicks);
        ImGui.TextColored(color, _label);
        ImGui.SameLine();

        var cur = ReadOnlySpan<byte>.Empty; 
        var prev = ReadOnlySpan<byte>.Empty;

        // if (IsActive)
        {
            cur = context.Memory.Read(Data.Address, Data.Size);
            if (context.Refreshed)
                prev = context.Memory.ReadPrevious(Data.Address, Data.Size);
        }

        ImGui.PushID(Name);
        var history = context.History.GetOrCreateHistory(Name, Data.Type);
        if (Data.Type.Draw(history, cur, prev, context))
            LastChangeTimeTicks = context.Now;
        ImGui.PopID();
    }
}