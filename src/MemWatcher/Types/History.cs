namespace MemWatcher.Types;

public class History
{
    public string Path { get; }
    public History(string path)
    {
        Path = path;
        LastModifiedTicks = DateTime.UtcNow.Ticks;
    }

    public long LastModifiedTicks { get; set; }
    public List<IDirective>? Directives { get; set; } // null = no directives or not yet initialised.
    public override string ToString() => $"H:{Path}:{Util.Timestamp(LastModifiedTicks):g3}";
    public static History DefaultConstructor(string path) => new(path) { LastModifiedTicks = DateTime.UtcNow.Ticks };
}