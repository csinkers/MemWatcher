using ImGuiNET;

namespace MemWatcher;

public sealed class WatcherCore : IDisposable
{
    readonly IMemoryReader _reader;
    readonly ProgramData _data;
    readonly DrawContext _drawContext;

    public string Filter { get; set; } = "";
    public Config Config { get; }
    public DateTime LastUpdateTimeUtc { get; private set; } = DateTime.MinValue;

    // Active set
    // Show available symbols + active symbols
    // Allow expanding arrays, slicing across single struct elem etc
    // Update mem data using background thread
    // Track value history?
    // Highlight changed values
    // Searching / filtering.

    public WatcherCore(string xmlFilename, IMemoryReader reader, Config config, TextureStore textures)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
        Config = config ?? throw new ArgumentNullException(nameof(config));
        var memory = new MemoryCache(_reader);
        var history = new HistoryCache();

        using (var xmlStream = File.OpenRead(xmlFilename))
            _data = new ProgramData(xmlStream);

        _drawContext = new DrawContext(memory, history, _data, textures);

        /* foreach (var name in config.Watches)
        {
            var index = name.IndexOf('/');
            var nsPart = index == -1 ? "" : name[..index];
            var watchPart = index == -1 ? name : name[(index + 1)..];
            var ns = _namespaces.FirstOrDefault(x => x.Name == nsPart);
            var watch = ns?.Watches.FirstOrDefault(x => x.Name == watchPart);
            if (watch != null)
                watch.IsActive = true;
        }*/
    }

    public void Draw()
    {
        if (ImGui.Button("Save Config"))
            SaveConfig();

        _drawContext.Now = DateTime.UtcNow.Ticks;
        _drawContext.Filter = Filter;
        var history = _drawContext.History.GetOrCreateHistory(Constants.RootNamespaceName, _data.Root);
        _data.Root.Draw(history, 0, ReadOnlySpan<byte>.Empty, ReadOnlySpan<byte>.Empty, _drawContext);
        _drawContext.Refreshed = false;
    }

    void SaveConfig()
    {
        // Config.Watches.Clear();
        // foreach (var ns in _data.Namespaces)
        //    foreach (var watch in ns.Watches/*.Where(watch => watch.IsActive)*/)
        //        Config.Watches.Add(ns.Name + "/" + watch.Name);
        Config.Save();
    }

    public void Update()
    {
        _drawContext.History.CycleHistory();
        _drawContext.Memory.Refresh();
        _drawContext.Refreshed = true;
        LastUpdateTimeUtc = DateTime.UtcNow;
    }

    public void Dispose() => _reader.Dispose();
}