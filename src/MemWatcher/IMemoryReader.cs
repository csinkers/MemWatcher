namespace MemWatcher;

public interface IMemoryReader : IDisposable
{
    byte[]? Read(uint offset, uint size);
}