using ImGuiNET;

namespace MemWatcher.Types;

public class GString : IGhidraType
{
    class StringHistory : History
    {
        public uint Size { get; set; }
        public StringHistory(string path) : base(path) { }
        public override string ToString() => $"StringH:{Path}:{Util.Timestamp(LastModifiedTicks):g3}";
    }

    const int MaxStringLength = 1024;
    const uint InitialSize = 32;
    GString() { }
    public static readonly GString Instance = new();
    public string Namespace => "/";
    public string Name => "string";
    public bool IsFixedSize => false;
    public uint GetSize(History? history) => ((StringHistory?)history)?.Size ?? InitialSize;
    public History HistoryConstructor(string path) => new StringHistory(path);

    public bool Draw(History history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
        => Draw((StringHistory)history, buffer, previousBuffer);

    static bool Draw(StringHistory history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer)
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
        else
            history.Size = (uint)zeroIndex + 1;

        if (zeroIndex == -1)
        {
            ImGui.TextUnformatted("");
            return false;
        }

        var text = Constants.Encoding.GetString(buffer[..zeroIndex]);
        ImGui.TextUnformatted("\"" + text + "\"");
        return !previousBuffer.IsEmpty && !buffer.SequenceEqual(previousBuffer);
    }

    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) { return false; }
}