namespace MemWatcher.Types;

public class History
{
    public History() => LastModifiedTicks = DateTime.UtcNow.Ticks;
    public long LastModifiedTicks { get; set; }
    public static History DefaultConstructor() => new() { LastModifiedTicks = DateTime.UtcNow.Ticks };
}