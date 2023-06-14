using System.Runtime.InteropServices;
using System.Security;

namespace MemWatcher;

internal static class NativeImports
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern uint NtReadVirtualMemory(IntPtr ProcessHandle, IntPtr BaseAddress, byte[]? Buffer,
        UInt32 NumberOfBytesToRead, ref UInt32 NumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    [SuppressUnmanagedCodeSecurity]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool CloseHandle(IntPtr hObject);

    public const int PROCESS_VM_OPERATION = 0x0008;
    public const int PROCESS_VM_READ = 0x0010;
    public const int PROCESS_VM_WRITE = 0x0020;
}