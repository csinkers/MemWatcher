using GhidraData;

namespace DataCorrelator;

static class Program
{
    record Data(uint Address, string Name, IGhidraType Type)
    {
        public string? CurrentName { get; set; }
        public IGhidraType CurrentType { get; set; }
    }

    public static void Main(string[] args)
    {
        var left = new ProgramData(args[0], x => x != "_" && !x.Contains(".DLL"));
        var right = new ProgramData(args[1], x => x != "CheckCookie");
        using var sw = new StreamWriter(args[2]);

        int adjustment = -0x3cd000;
        var globals = new Dictionary<uint, Data>();
        foreach (var member in left.Root.Members)
        {
            if (member is not GGlobal global)
                continue;

            var address = (uint)(global.Address + adjustment);
            globals[address] = new(address, global.Key.Name, global.Type);
        }

        foreach (var member in right.Root.Members)
        {
            if (member is not GGlobal global)
                continue;

            if (!globals.TryGetValue(global.Address, out var data))
            {
                continue;
            }

            data.CurrentName = global.Key.Name;
            data.CurrentType = global.Type;
        }

        foreach (var data in globals.Values.OrderBy(x => x.Address))
        {
            if (data.Type.Key.Name.Contains("undefined"))
                continue;

            if (data.Type.ToString() == data.CurrentType?.ToString())
                continue;

            sw.WriteLine($"        SetType(0x{data.Address:X8}, \"{data.Name}\", \"{data.Type}\"); // was {data.CurrentType?.ToString() ?? "[MISSING]"}");
        }
    }
}
