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
    public byte[]? LastBuffer { get; set; }
    public override string ToString() => $"[{(IsActive ? 'x' : ' ')}] {Name}: {Data}";

    public void Draw(SymbolLookup lookup)
    {
        var active = IsActive;
        ImGui.Checkbox(CheckboxId, ref active);
        IsActive = active;
        ImGui.SameLine();

        ImGui.Text(Name + ": ");
        ImGui.SameLine();
        Data.Type.Draw(Name, LastBuffer ?? Array.Empty<byte>(), lookup);
    }

    public void Update(MemoryReader reader)
    {
        if (!IsActive)
        {
            LastBuffer = null;
            return;
        }

        LastBuffer = reader.Read(Data.Address, Data.Size);
    }
}