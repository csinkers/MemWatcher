using MemWatcher.Types;

namespace MemWatcher;

public class HistoryCache
{
    static readonly TimeSpan CycleInterval = TimeSpan.FromSeconds(5);
    Dictionary<string, History> _oldHistory = new();
    Dictionary<string, History> _history = new();
    DateTime _lastCycleTime;

    public History GetHistory(string path, IGhidraType type)
    {
        if (_history.TryGetValue(path, out var history)) // Was used recently
            return history;

        if (!_oldHistory.TryGetValue(path, out history))
            history = type.HistoryConstructor(path); // Wasn't used in the current or the previous cache

        _history[path] = history; // Wasn't used in the current cache, so put it in

        return history;
    }

    public void CycleHistory()
    {
        if (DateTime.UtcNow - _lastCycleTime <= CycleInterval) 
            return;

        _oldHistory = _history;
        _history = new Dictionary<string, History>();
        _lastCycleTime = DateTime.UtcNow;
    }
}