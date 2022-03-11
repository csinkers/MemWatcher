using System.Text;

namespace MemWatcher;

public static class Constants
{
    public const uint PointerSize = 4;
    public static Encoding Encoding { get; }

    static Constants()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance); // Required for code page 850 support in .NET Core
        Encoding = Encoding.GetEncoding(850);
    }
}