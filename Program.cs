using System.Diagnostics;
using System.Numerics;
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

        var core = new WatcherCore(SymbolPath, ProcessName);
        var sw = new Stopwatch();
        int interval = 100;
        bool onlyShowActive = false;

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
                var active = core.ActiveWatches;
                core.Dispose();
                core = new WatcherCore(SymbolPath, ProcessName);
                core.ActiveWatches = active;
            }
            ImGui.SameLine();
            ImGui.Checkbox("Only Show Active", ref onlyShowActive);

            ImGui.DragInt("Refresh Interval (ms)", ref interval, 1.0f, 100, 5000);
            ImGui.Text($"Last refresh took {sw.ElapsedMilliseconds} ms");

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