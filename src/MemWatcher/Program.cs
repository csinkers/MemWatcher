using System.Diagnostics;
using System.Numerics;
using System.Text;
using ImGuiNET;
using Veldrid;
using Veldrid.StartupUtilities;

namespace MemWatcher;

public static class Program
{
    const string SymbolPath = @"C:\Depot\bb\ualbion_extra\SR-Main.exe.xml";
    const string ProcessName = @"SR-Main";
    public static void Main()
    {
        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(100, 100, 800, 1024, WindowState.Normal, "MemWatcher"),
            out var window,
            out var gd);
        gd.SyncToVerticalBlank = true;

        var imguiRenderer = new ImGuiRenderer(
            gd, gd.MainSwapchain.Framebuffer.OutputDescription,
            (int)gd.MainSwapchain.Framebuffer.Width, (int)gd.MainSwapchain.Framebuffer.Height);

        var cl = gd.ResourceFactory.CreateCommandList();
        window.Resized += () =>
        {
            gd.ResizeMainWindow((uint)window.Width, (uint)window.Height);
            imguiRenderer.WindowResized(window.Width, window.Height);
        };

        var config = Config.Load();
        var reader = MemoryReader.Attach(ProcessName);
        var core = new WatcherCore(SymbolPath, reader, config);
        var sw = new Stopwatch();
        int interval = 100;
        bool onlyShowActive = false;

        var filterBuf = new byte[128];
        while (window.Exists)
        {
            var input = window.PumpEvents();
            if (!window.Exists)
                break;

            imguiRenderer.Update(1f / 60f, input);

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(window.Width, window.Height));
            ImGui.Begin("Watcher");

            if (ImGui.Button("Reload from XML"))
            {
                core.Dispose();
                reader.Dispose();

                reader = MemoryReader.Attach(ProcessName);
                core = new WatcherCore(SymbolPath, reader, config);
            }
            ImGui.SameLine();
            ImGui.Checkbox("Only Show Active", ref onlyShowActive);

            ImGui.Columns(2);
            ImGui.DragInt("Interval (ms)", ref interval, 1.0f, 1, 5000);
            ImGui.NextColumn();
            ImGui.TextUnformatted($"Last refresh took {sw.ElapsedMilliseconds} ms");
            ImGui.Columns(1);

            if (ImGui.InputText("Filter", filterBuf, (uint)filterBuf.Length))
            {
                var raw = Encoding.UTF8.GetString(filterBuf);
                int zeroIndex = raw.IndexOf('\0');
                core.Filter = zeroIndex == -1 ? raw : raw[..zeroIndex];
            }

            if ((DateTime.UtcNow - core.LastUpdateTimeUtc).TotalMilliseconds > interval)
            {
                sw.Reset();
                sw.Start();
                core.Update();
                sw.Stop();
            }

            ImGui.BeginChild("Data");
            core.Draw(onlyShowActive);
            ImGui.EndChild();

            ImGui.End();

            cl.Begin();
            cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
            cl.ClearColorTarget(0, RgbaFloat.Black);
            imguiRenderer.Render(gd, cl);
            cl.End();
            gd.SubmitCommands(cl);
            gd.SwapBuffers(gd.MainSwapchain);
        }

        core.Dispose();
    }
}