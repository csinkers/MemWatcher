using System.Numerics;
using ImGuiNET;

namespace MemWatcher.Types;

public class GPrimitive : IGhidraType
{
    public delegate void DrawFunc(ReadOnlySpan<byte> buffer, Vector4 color);
    readonly DrawFunc _drawFunc;
    readonly uint _size;

    public GPrimitive(string name, uint size, DrawFunc drawFunc)
    {
        _drawFunc = drawFunc ?? throw new ArgumentNullException(nameof(drawFunc));
        Name = name;
        _size = size;
    }

    public string Namespace => "/";
    public string Name { get; }
    public bool IsFixedSize => true;
    public uint GetSize(History? history) => _size;
    public History HistoryConstructor(string path) => History.DefaultConstructor(path);

    public override string ToString() => Name;
    public bool Draw(History history, ReadOnlySpan<byte> buffer, ReadOnlySpan<byte> previousBuffer, DrawContext context)
    {
        if (_size == 0)
        {
            ImGui.TextUnformatted("");
            return false;
        }

        if (buffer.Length < _size)
        {
            ImGui.TextUnformatted("--");
            return false;
        }

        if (!previousBuffer.IsEmpty && !buffer.SequenceEqual(previousBuffer))
            history.LastModifiedTicks = context.Now;

        var color = Util.ColorForAge(context.Now - history.LastModifiedTicks);
        _drawFunc(buffer, color);
        return history.LastModifiedTicks == context.Now;
    }

    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) { return false; }
    public static GPrimitive Void { get; } = new("void", 0, DrawVoid);
    public static GPrimitive Char { get; } = new("char", 1, DrawString);

    public static readonly GPrimitive[] PrimitiveTypes = {
        new("bool",       1, DrawBool),

        new("sbyte",      1, DrawInt1),
        new("word",       2, DrawInt2),
        new("short",      2, DrawInt2),
        new("int",        4, DrawInt4),
        new("long",       4, DrawInt4),
        new("dword",      4, DrawInt4),
        new("longlong",   8, DrawInt8),
        new("qword",      8, DrawInt8),

        new("byte",       1, DrawUInt1),
        new("uchar",      1, DrawUInt1),
        new("ushort",     2, DrawUInt2),
        new("uint",       4, DrawUInt4),
        new("ulong",      4, DrawUInt4),
        new("ulonglong",  8, DrawUInt8),

        new("undefined",  1, DrawUInt1),
        new("undefined1", 1, DrawUInt1),
        new("undefined2", 2, DrawUInt2),
        new("undefined4", 4, DrawUInt4),
        new("undefined6", 6, DrawUInt6),
        new("undefined8", 8, DrawUInt8),

        Char,
        new("float",      4, DrawFloat),
        new("double",     8, DrawDouble),
        new("float10",   10, DrawFloat10),

        Void,
        new("va_list",    0, DrawList),

        new("ImageBaseOffset32", 4, DrawUInt4),
        new("pointer32", 4, DrawUInt4),
        new("pointer", Constants.PointerSize, Constants.PointerSize == 8 ? DrawUInt8 : DrawUInt4),
        new("size_t", Constants.PointerSize, Constants.PointerSize == 8 ? DrawUInt8 : DrawUInt4),
    };

    static void DrawFloat10(ReadOnlySpan<byte> buffer, Vector4 color) => ImGui.TextColored(color, "float10");
    static void DrawDouble(ReadOnlySpan<byte> buffer, Vector4 color) => ImGui.TextColored(color, ((float)BitConverter.ToDouble(buffer)).ToString("g3"));
    static void DrawFloat(ReadOnlySpan<byte> buffer, Vector4 color) => ImGui.TextColored(color, BitConverter.ToSingle(buffer).ToString("g3"));
    static void DrawInt1(ReadOnlySpan<byte> buffer, Vector4 color) => ImGui.TextColored(color, ((sbyte)buffer[0]).ToString());
    static void DrawInt2(ReadOnlySpan<byte> buffer, Vector4 color) => ImGui.TextColored(color, BitConverter.ToInt16(buffer).ToString());
    static void DrawInt4(ReadOnlySpan<byte> buffer, Vector4 color) => ImGui.TextColored(color, BitConverter.ToInt32(buffer).ToString());
    static void DrawInt8(ReadOnlySpan<byte> buffer, Vector4 color) => ImGui.TextColored(color, BitConverter.ToInt64(buffer).ToString());

    static void DrawUInt1(ReadOnlySpan<byte> buffer, Vector4 color)
    {
        var value = buffer[0];
        ImGui.TextColored(color, $"{value} ({value:X})");
    }

    static void DrawUInt2(ReadOnlySpan<byte> buffer, Vector4 color)
    {
        var value = BitConverter.ToUInt16(buffer);
        ImGui.TextColored(color, $"{value} ({value:X})");
    }

    static void DrawUInt4(ReadOnlySpan<byte> buffer, Vector4 color)
    {
        var value = BitConverter.ToUInt32(buffer);
        ImGui.TextColored(color, $"{value} ({value:X})");
    }

    static void DrawUInt6(ReadOnlySpan<byte> buffer, Vector4 color) => ImGui.TextColored(color, "undefined6");
    static void DrawUInt8(ReadOnlySpan<byte> buffer, Vector4 color)
    {
        var value = BitConverter.ToUInt64(buffer);
        ImGui.TextColored(color, $"{value} ({value:X})");
    }

    static void DrawList(ReadOnlySpan<byte> buffer, Vector4 color) => ImGui.TextColored(color, "va_list");
    static void DrawVoid(ReadOnlySpan<byte> buffer, Vector4 color) => ImGui.TextColored(color, "void");
    static void DrawString(ReadOnlySpan<byte> buffer, Vector4 color) => ImGui.TextColored(color, Constants.Encoding.GetString(buffer));
    static void DrawBool(ReadOnlySpan<byte> buffer, Vector4 color)
    {
        switch (buffer.Length)
        {
            case 1: ImGui.TextColored(color, buffer[0] == 0 ? "false" : "true"); break;
            case 4: ImGui.TextColored(color, BitConverter.ToUInt32(buffer) == 0 ? "false" : "true"); break;
            default: ImGui.TextColored(color, $"bool len {buffer.Length}"); break;
        }
    }
}