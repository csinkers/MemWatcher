namespace CorrelateSymbols;

public class GPrimitive : IGhidraType
{
    readonly uint _size;
    readonly string _name;

    public GPrimitive(string name, uint size)
    {
        _name = name;
        _size = size;
    }

    public TypeKey Key => new("/", _name);
    public bool IsFixedSize => true;

    public override string ToString() => _name;

    public bool Unswizzle(Dictionary<TypeKey, IGhidraType> types) { return false; }
    public static GPrimitive Void { get; } = new("void", 0);
    public static GPrimitive Char { get; } = new("char", 1);

    public static readonly GPrimitive[] PrimitiveTypes = {
        new("bool",       1),

        new("sbyte",      1),
        new("word",       2),
        new("short",      2),
        new("int",        4),
        new("long",       4),
        new("dword",      4),
        new("longlong",   8),
        new("qword",      8),

        new("byte",       1),
        new("uchar",      1),
        new("ushort",     2),
        new("uint",       4),
        new("ulong",      4),
        new("ulonglong",  8),

        new("undefined",  1),
        new("undefined1", 1),
        new("undefined2", 2),
        new("undefined4", 4),
        new("undefined6", 6),
        new("undefined8", 8),

        Char,
        new("float",      4),
        new("double",     8),
        new("float10",   10),

        Void,
        new("va_list",    0),

        new("pointer32", 4),
        new("pointer", Constants.PointerSize),
        new("size_t", Constants.PointerSize),
    };
}