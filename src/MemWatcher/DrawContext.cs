namespace MemWatcher;

public class DrawContext
{
    public DrawContext(MemoryCache memory, HistoryCache history, SymbolLookup lookup)
    {
        Memory = memory ?? throw new ArgumentNullException(nameof(memory));
        History = history ?? throw new ArgumentNullException(nameof(history));
        Lookup = lookup ?? throw new ArgumentNullException(nameof(lookup));
    }

    public MemoryCache Memory { get; }
    public HistoryCache History { get; }
    public SymbolLookup Lookup { get; }
    public long Now { get; set; }
    public bool Refreshed { get; set; }
    public float SinceStart => Util.Timestamp(Now);
}