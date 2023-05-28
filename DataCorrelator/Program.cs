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
        var left = ProgramData.Load(args[0], x => x != "_" && !x.Contains(".DLL"));
        var right = ProgramData.Load(args[1], x => x != "CheckCookie");

        left.PopulateCalls(Path.ChangeExtension(args[0], ".txt"));
        right.PopulateCalls(Path.ChangeExtension(args[1], ".txt"));

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
