using ImGuiNET;

namespace MemWatcher.Types;

public class GString : IGhidraType
{
    const int MaxStringLength = 1024;
    const uint InitialSize = 32;
    GString() { }
    public static readonly GString Instance = new();
    public string Namespace => "/";
    public string Name => "string";
    public bool IsFixedSize => false;
    public uint GetSize(History? history) => ((StringHistory?)history)?.Size ?? InitialSize;
    public History HistoryConstructor() => new StringHistory();
    public bool Draw(string path, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, long now, SymbolLookup lookup)
    {
        int zeroIndex = -1;
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] == 0)
            {
                zeroIndex = i;
                break;
            }
        }

        var history = (StringHistory)lookup.GetHistory(path, this);
        if (zeroIndex == -1)
        {
            if (history.Size == 0)
                history.Size = InitialSize;
            else
                history.Size *= 2;

            if (history.Size > MaxStringLength)
                history.Size = MaxStringLength;

            zeroIndex = buffer.Length - 1;
        }

        if (zeroIndex == -1)
        {
            ImGui.Text("");
            return false;
        }

        history.Size = (uint)zeroIndex + 1;

        ImGui.Text(Constants.Encoding.GetString(buffer[..zeroIndex]));
        return buffer.SequenceEqual(previousBuffer);
    }

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) { }
}