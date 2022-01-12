namespace MemWatcher;

public class WatchNamespace
{
    public WatchNamespace(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));
    public string Name { get; }
    public List<Watch> Watches { get; } = new();
    public override string ToString() => $"{Name} ({Watches.Count})";
}