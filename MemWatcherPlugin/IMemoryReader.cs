namespace MemWatcherPlugin;

public interface IMemoryReader : IDisposable
{
    void Read(uint offset, byte[] buffer);
}