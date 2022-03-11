using ImGuiNET;

namespace MemWatcher;

public sealed class WatcherCore : IDisposable
{
    readonly List<WatchNamespace> _namespaces;
    readonly IMemoryReader _reader;
    readonly ProgramData _data;

    public string? Filter { get; set; }
    public Config Config { get; }
    public DateTime LastUpdateTimeUtc { get; private set; } = DateTime.MinValue;

    // Active set
    // Show available symbols + active symbols
    // Allow expanding arrays, slicing across single struct elem etc
    // Update mem data using background thread
    // Track value history?
    // Highlight changed values
    // Searching / filtering.

    public WatcherCore(string xmlFilename, IMemoryReader reader, Config config)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        Config = config ?? throw new ArgumentNullException(nameof(config));

        using(var xmlStream = File.OpenRead(xmlFilename))
            _data = new ProgramData(xmlStream);

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

        foreach (var name in config.Watches)
        {
            var index = name.IndexOf('/');
            var nsPart = index == -1 ? "" : name[..index];
            var watchPart = index == -1 ? name : name[(index + 1)..];
            var ns = _namespaces.FirstOrDefault(x => x.Name == nsPart);
            var watch = ns?.Watches.FirstOrDefault(x => x.Name == watchPart);
            if (watch != null)
                watch.IsActive = true;
        }

    }

    bool IsShown(Watch watch, bool onlyShowActive)
    {
        if (!onlyShowActive && string.IsNullOrEmpty(Filter))
            return true;

        if (onlyShowActive && watch.IsActive)
            return true;

        if (!string.IsNullOrEmpty(Filter) && watch.Name.Contains(Filter, StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    public void Draw(bool onlyShowActive)
    {
        if (ImGui.Button("Save Config"))
            SaveConfig();

        foreach (var ns in _namespaces)
        {
            if (!ImGui.TreeNode(ns.Name))
                continue;

            foreach (var watch in ns.Watches)
                if (IsShown(watch, onlyShowActive))
                    watch.Draw(_data.SymbolLookup);

            ImGui.TreePop();
        }
    }

    void SaveConfig()
    {
        Config.Watches.Clear();
        foreach (var ns in _namespaces)
            foreach (var watch in ns.Watches.Where(watch => watch.IsActive))
                Config.Watches.Add(ns.Name + "/" + watch.Name);
        Config.Save();
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