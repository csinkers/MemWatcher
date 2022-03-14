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
    public override string ToString() => $"H:{Path}:{Util.Timestamp(LastModifiedTicks):g3}";
    public static History DefaultConstructor(string path) => new(path) { LastModifiedTicks = DateTime.UtcNow.Ticks };
}