using System.Numerics;
using System.Runtime.InteropServices;
using ImGuiNET;

namespace MemWatcher.Types;

public class GGraphics : IGhidraType
{
    public GGraphics(string width, string height, string stride, string palette)
    {
        Width = width;
        Height = height;
        Stride = stride;
        Palette = palette;
    }

    class GraphicsHistory : History
    {
        public GraphicsHistory(string path, IGhidraType type, string? width, string? height, string? stride, string? palette) : base(path, type)
        {
            Width = width;
            Height = height;
            Stride = stride;
            Palette = palette;
        }
        public int? TextureHandle { get; set; }
        public uint LastCheckSum { get; set; }

        // Resolved history absolute paths
        public string? Width { get; }
        public string? Height { get; }
        public string? Stride { get; }
        public string? Palette { get; }
    }

    // Logical relative paths
    public string Width { get; }
    public string Height { get; }
    public string Stride { get; }
    public string Palette { get; }

    public string Namespace => Constants.SpecialNamespace;
    public string Name => "gfx";
    public bool IsFixedSize => false;

    public uint GetSize(History? history) => Constants.PointerSize;
    public History HistoryConstructor(string path, Func<string, string, string?> resolvePath) =>
        new GraphicsHistory(
            path,
            this,
            resolvePath(Width, path),
            resolvePath(Height, path),
            resolvePath(Stride, path),
            resolvePath(Palette, path));

    public bool Draw(History history, uint address, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        history.LastAddress = address;
        if (buffer.Length < Constants.PointerSize)
        {
            ImGui.TextUnformatted("-GFX-");
            return false;
        }

        if (!previousBuffer.IsEmpty && !buffer.SequenceEqual(previousBuffer))
            history.LastModifiedTicks = context.Now;

        var h = (GraphicsHistory)history;
        uint width = context.ReadUShort(h.Width);
        uint height = context.ReadUShort(h.Height);
        uint stride = context.ReadUShort(h.Stride);
        if (stride == 0)
            stride = width;

        var rawAddress = BitConverter.ToUInt32(buffer);

        ImGui.TextUnformatted($"-GFX {width}x{height} @ {rawAddress:X}-");

        var paletteBuf = context.ReadBytes(h.Palette, 256 * 4);
        var pixelData = context.Memory.Read(rawAddress, width * height);
        if (paletteBuf.IsEmpty || pixelData.IsEmpty)
        {
            ImGui.Text("!! NO IMG !!");
            return false;
        }

        uint sum = 0;
        foreach (var b in paletteBuf) sum = unchecked(sum + b);
        foreach (var b in pixelData) sum = unchecked(sum + b);

        var (handle, texture) = context.TextureStore.Get(h.TextureHandle, width, height);
        if (handle != h.TextureHandle || sum != h.LastCheckSum)
        {
            context.TextureStore.Update(texture, width, height, (int)stride, pixelData, MemoryMarshal.Cast<byte, uint>(paletteBuf));
            h.TextureHandle = handle;
            h.LastCheckSum = sum;
        }

        var imguiBinding = context.TextureStore.GetImGuiBinding(handle);
        if (imguiBinding == IntPtr.Zero)
            ImGui.Text("!! NO IMG !!");
        else
            ImGui.Image(imguiBinding, new Vector2(width, height));

        return false;
    }

    public string? BuildPath(string accum, string relative) => null;
    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) => false;
}

