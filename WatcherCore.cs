using ImGuiNET;

namespace MemWatcher;

public sealed class WatcherCore : IDisposable
{
    readonly List<WatchNamespace> _namespaces;
    readonly MemoryReader _reader;
    readonly ProgramData _data;

    public DateTime LastUpdateTimeUtc { get; private set; } = DateTime.MinValue;
    public HashSet<(string, string)> ActiveWatches {
        get
        {
            var result = new HashSet<(string, string)>();
            foreach (var ns in _namespaces)
                foreach (var watch in ns.Watches)
                    if(watch.IsActive)
                        result.Add((ns.Name, watch.Name));
            return result;
        }
        set
        {
            foreach (var ns in _namespaces)
                foreach (var watch in ns.Watches)
                    if (value.Contains((ns.Name, watch.Name)))
                        watch.IsActive = true;
        }
    }

    // Active set
    // Show available symbols + active symbols
    // Allow expanding arrays, slicing across single struct elem etc
    // Update mem data using background thread
    // Track value history?
    // Highlight changed values
    // Searching / filtering.

    public WatcherCore(string xmlFilename, string processName)
    {
        _data = new ProgramData(xmlFilename);
        var dict = new Dictionary<string, WatchNamespace>();
        foreach (var kvp in _data.Data)
        {
            if (!dict.TryGetValue(kvp.Key.Item1, out var ns))
            {
                ns = new WatchNamespace(kvp.Key.Item1);
                dict[kvp.Key.Item1] = ns;
            }

            ns.Watches.Add(new Watch(kvp.Key.Item2, kvp.Value));
        }

        _namespaces = dict.Values.OrderBy(x => x.Name).ToList();
        foreach (var ns in _namespaces)
            ns.Watches.Sort((x, y) => Comparer<string>.Default.Compare(x.Name, y.Name));

        _reader = MemoryReader.Attach(processName);
    }

    public void Draw(bool onlyShowActive)
    {
        foreach (var ns in _namespaces)
        {
            if (ImGui.TreeNode(ns.Name))
            {
                foreach (var watch in ns.Watches)
                {
                    if (onlyShowActive && !watch.IsActive)
                        continue;
                    watch.Draw(_data.SymbolLookup);
                }

                ImGui.TreePop();
            }
        }
    }

    public void Update()
    {
        LastUpdateTimeUtc = DateTime.UtcNow;
        foreach (var ns in _namespaces)
            foreach (var watch in ns.Watches)
                watch.Update(_reader);
    }

    public void Dispose() => _reader.Dispose();
}