using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MemWatcher;

public sealed class MemoryReader : IMemoryReader
{
    readonly IntPtr _handle;
    readonly Process _process;

    public static MemoryReader Attach(string name)
    {
        var process = Process.GetProcessesByName(name).FirstOrDefault();

        if (process == null)
            throw new InvalidOperationException($"Could not open process \"{name}\"");

        var processHandle = NativeImports.OpenProcess(
            NativeImports.PROCESS_VM_OPERATION | NativeImports.PROCESS_VM_READ,
            false,
            process.Id);

        if (processHandle == IntPtr.Zero)
            throw new InvalidOperationException($"Could not connect to {name} process");

        return new MemoryReader(process, processHandle);
    }

    MemoryReader(Process process, IntPtr handle)
    {
        _process = process ?? throw new ArgumentNullException(nameof(process));
        _handle = handle;
    }

    public byte[]? Read(uint offset, uint size)
    {
        uint bytesRead = 0;
        var buffer = new byte[size];
        NativeImports.NtReadVirtualMemory(_handle, (IntPtr)offset, buffer, size, ref bytesRead);
        return buffer;
    }

    public uint GetModuleAddress(string name)
    {
        foreach (ProcessModule module in _process.Modules)
            if (module.ModuleName == name)
                return (uint)module.BaseAddress;
        return 0;
    }

    static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
    {
        unsafe
        {
            fixed (byte* ptr = &bytes[0])
            {
                return Marshal.PtrToStructure<T>((IntPtr)ptr);
            }
        }
    }

    static byte[] StructureToByteArray<T>(in T obj) where T : struct
    {
        var length = Marshal.SizeOf(obj);
        var pointer = Marshal.AllocHGlobal(length);
        var array = new byte[length];

        Marshal.StructureToPtr(obj, pointer, true);
        Marshal.Copy(pointer, array, 0, length);
        Marshal.FreeHGlobal(pointer);

        return array;
    }

    public void Dispose()
    {
        NativeImports.CloseHandle(_handle);
        _process.Dispose();
    }
}