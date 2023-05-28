using GhidraData;

namespace MemWatcher;

public class DrawContext
{
    public DrawContext(MemoryCache memory, HistoryCache history, ProgramData lookup, TextureStore textureStore, RendererCache renderers)
    {
        Memory = memory ?? throw new ArgumentNullException(nameof(memory));
        History = history ?? throw new ArgumentNullException(nameof(history));
        Lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));
        TextureStore = textureStore ?? throw new ArgumentNullException(nameof(textureStore));
        Renderers = renderers ?? throw new ArgumentNullException(nameof(renderers));
    }

    public RendererCache Renderers { get; }
    public MemoryCache Memory { get; }
    public HistoryCache History { get; }
    public ProgramData Lookup { get; }
    public TextureStore TextureStore { get; }
    public long Now { get; set; }
    public bool Refreshed { get; set; }
    public float SinceStart => Util.Timestamp(Now);
    public string Filter { get; set; } = "";

    public ushort ReadUShort(string? path)
    {
        var bytes = ReadBytes(path, 2);
        return bytes.Length != 2 ? (ushort)0 : BitConverter.ToUInt16(bytes);
    }

    public uint ReadUInt(string? path)
    {
        var bytes = ReadBytes(path, 4);
        return bytes.Length != 4 ? 0 : BitConverter.ToUInt32(bytes);
    }

    public ReadOnlySpan<byte> ReadBytes(string? path, uint size)
    {
        if (path == null)
            return ReadOnlySpan<byte>.Empty;

        var history = History.TryGetHistory(path);
        return history == null ? ReadOnlySpan<byte>.Empty : Memory.Read(history.LastAddress, size);
    }
}