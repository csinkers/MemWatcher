﻿using System.Diagnostics;
using System.Numerics;
using System.Text;
using ImGuiNET;
using MemWatcherPlugin;
using Veldrid;
using Veldrid.StartupUtilities;

namespace MemWatcher;

public static class Program
{
    const string ProcessName = @"SR-Main";
    public static void Main()
    {
#if RENDERDOC
        RenderDoc.Load(out var renderDoc);
        bool capturePending = false;
#endif

        VeldridStartup.CreateWindowAndGraphicsDevice(
            new WindowCreateInfo(100, 100, 800, 1024, WindowState.Normal, "MemWatcher"),
            new GraphicsDeviceOptions(true) { SyncToVerticalBlank = true },
            GraphicsBackend.Direct3D11,
            out var window,
            out var gd);

        var imguiRenderer = new ImGuiRenderer(
            gd,
            gd.MainSwapchain.Framebuffer.OutputDescription,
            (int)gd.MainSwapchain.Framebuffer.Width,
            (int)gd.MainSwapchain.Framebuffer.Height);

        var cl = gd.ResourceFactory.CreateCommandList();
        window.Resized += () =>
        {
            gd.ResizeMainWindow((uint)window.Width, (uint)window.Height);
            imguiRenderer.WindowResized(window.Width, window.Height);
        };

        var textures = new TextureStore(gd, imguiRenderer);
        var reader = WindowsMemoryReader.Attach(ProcessName);
        var core = new WatcherCore(reader, textures);
        var sw = new Stopwatch();
        int interval = 100;
        // bool onlyShowActive = false;

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));

        var filterBuf = new byte[128];
        while (window.Exists)
        {
#if RENDERDOC
            if (capturePending)
            {
                renderDoc.TriggerCapture();
                capturePending = false;
            }
#endif

            textures.Cycle();
            var input = window.PumpEvents();
            if (!window.Exists)
                break;

            imguiRenderer.Update(1f / 60f, input);

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(window.Width, window.Height));
            ImGui.Begin("Watcher");

#if RENDERDOC
            ImGui.SameLine();
            if (ImGui.Button("RenderDoc Snapshot"))
                capturePending = true;

            ImGui.SameLine();
            if (ImGui.Button("Open RenderDoc"))
                renderDoc.LaunchReplayUI();
#endif

            // ImGui.SameLine();
            // ImGui.Checkbox("Only Show Active", ref onlyShowActive);

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
            core.Draw();
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