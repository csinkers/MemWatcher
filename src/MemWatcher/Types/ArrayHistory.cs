namespace MemWatcher.Types;

public class ArrayHistory : History
{
    public ArrayHistory(string?[] paths) => Paths = paths ?? throw new ArgumentNullException(nameof(paths));
    public string?[] Paths { get; }
}