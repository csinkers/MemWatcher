using ImGuiNET;
using MemWatcher.Types;

namespace MemWatcher;

public class Watch
{
    public Watch(string name, GData data)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Data = data ?? throw new ArgumentNullException(nameof(data));
        CheckboxId = Data.Address.ToString("X8");
    }

    string CheckboxId { get; }
    public string Name { get; }
    public GData Data { get; }
    public bool IsActive { get; set; }
    public byte[]? PreviousBuffer { get; set; }
    public byte[]? CurrentBuffer { get; set; }
    public long LastChangeTimeTicks { get; set; }
    public override string ToString() => $"[{(IsActive ? 'x' : ' ')}] {Name}: {Data}";

    public void Draw(SymbolLookup lookup)
    {
        var active = IsActive;
        ImGui.Checkbox(CheckboxId, ref active);
        IsActive = active;
        ImGui.SameLine();

        long now = DateTime.UtcNow.Ticks;
        var color = Util.ColorForAge(now - LastChangeTimeTicks);
        ImGui.TextColored(color, Name + ": ");
        ImGui.SameLine();

        ImGui.PushID(Name);
        if (Data.Type.Draw(Name, CurrentBuffer ?? Array.Empty<byte>(), PreviousBuffer ?? Array.Empty<byte>(), now, lookup))
            LastChangeTimeTicks = now;
        ImGui.PopID();
    }

    public void Update(IMemoryReader reader)
    {
        PreviousBuffer = CurrentBuffer; 

        if (!IsActive)
        {
            CurrentBuffer = null;
            return;
        }

        CurrentBuffer = reader.Read(Data.Address, Data.Size);
    }
}