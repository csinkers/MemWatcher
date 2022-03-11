using ImGuiNET;

namespace MemWatcher.Types;

public class GPrimitive : IGhidraType
{
    public delegate void DrawFunc(ReadOnlySpan<byte> buffer);
    readonly DrawFunc _drawFunc;

    public GPrimitive(string name, uint size, DrawFunc drawFunc)
    {
        _drawFunc = drawFunc ?? throw new ArgumentNullException(nameof(drawFunc));
        Name = name;
        Size = size;
    }

    public string Namespace => "/";
    public string Name { get; }
    public uint Size { get; }

    public override string ToString() => Name;
    public void Draw(string path, ReadOnlySpan<byte> buffer, SymbolLookup lookup)
    {
        if (buffer.Length < Size)
        {
            ImGui.Text("--");
            return;
        }

        _drawFunc(buffer);
    }

    public void Unswizzle(Dictionary<(string ns, string name), IGhidraType> types) { }

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
        new("string",     0, DrawString),

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

    static void DrawFloat10(ReadOnlySpan<byte> buffer) => ImGui.Text("float10");
    static void DrawDouble(ReadOnlySpan<byte> buffer) => ImGui.Value("", (float)BitConverter.ToDouble(buffer));
    static void DrawFloat(ReadOnlySpan<byte> buffer) => ImGui.Value("", BitConverter.ToSingle(buffer));
    static void DrawInt1(ReadOnlySpan<byte> buffer) => ImGui.Value("", (sbyte)buffer[0]);
    static void DrawInt2(ReadOnlySpan<byte> buffer) => ImGui.Value("", BitConverter.ToInt16(buffer));
    static void DrawInt4(ReadOnlySpan<byte> buffer) => ImGui.Value("", BitConverter.ToInt32(buffer));
    static void DrawInt8(ReadOnlySpan<byte> buffer) => ImGui.Value("", BitConverter.ToInt64(buffer));

    static void DrawUInt1(ReadOnlySpan<byte> buffer)
    {
        var value = buffer[0];
        ImGui.Text($"{value} ({value:X})");
    }

    static void DrawUInt2(ReadOnlySpan<byte> buffer)
    {
        var value = BitConverter.ToUInt16(buffer);
        ImGui.Text($"{value} ({value:X})");
    }

    static void DrawUInt4(ReadOnlySpan<byte> buffer)
    {
        var value = BitConverter.ToUInt32(buffer);
        ImGui.Text($"{value} ({value:X})");
    }

    static void DrawUInt6(ReadOnlySpan<byte> buffer) => ImGui.Text("undefined6");
    static void DrawUInt8(ReadOnlySpan<byte> buffer)
    {
        var value = BitConverter.ToUInt64(buffer);
        ImGui.Text($"{value} ({value:X})");
    }

    static void DrawList(ReadOnlySpan<byte> buffer) => ImGui.Text("va_list");
    static void DrawVoid(ReadOnlySpan<byte> buffer) => ImGui.Text("void");
    static void DrawString(ReadOnlySpan<byte> buffer) => ImGui.Text(Constants.Encoding.GetString(buffer));
    static void DrawBool(ReadOnlySpan<byte> buffer)
    {
        switch (buffer.Length)
        {
            case 1: ImGui.Text(buffer[0] == 0 ? "false" : "true"); break;
            case 4: ImGui.Text(BitConverter.ToUInt32(buffer) == 0 ? "false" : "true"); break;
            default: ImGui.Text($"bool len {buffer.Length}"); break;
        }
    }
}