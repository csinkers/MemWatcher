﻿namespace MemWatcher.Types;

public class GData
{
    public uint Address { get; }
    public uint Size { get; }
    public IGhidraType Type { get; private set; }
    public override string ToString() => $"{Type} @ {Address:X} ({Size:X})";

    public GData(uint address, uint size, IGhidraType type)
    {
        Address = address;
        Size = size;
        Type = type ?? throw new ArgumentNullException(nameof(type));
    }

    public bool Unswizzle(Dictionary<(string ns, string name), IGhidraType> types)
    {
        if (Type is not GDummy dummy) 
            return false;

        Type = types[(dummy.Namespace, dummy.Name)];
        return true;
    }
}