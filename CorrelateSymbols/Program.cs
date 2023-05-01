using System.Numerics;
using ImGuiNET;
using Veldrid;
using Veldrid.StartupUtilities;

namespace CorrelateSymbols;

public static class Program
{
    public static void Main(string[] args)
    {
        var left = new ProgramData(args[0], x => x != "_" && !x.Contains(".DLL"));
        var right = new ProgramData(args[1], x => x != "CheckCookie");
        var correlator = new Correlator(left, right, args[2]);

        DumpFunctions(left, Path.ChangeExtension(args[0], ".result"));
        DumpFunctions(right, Path.ChangeExtension(args[1], ".result"));

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

        // bool onlyShowActive = false;

        ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.1f, 0.1f, 0.1f, 1.0f));

        while (window.Exists)
        {
#if RENDERDOC
            if (capturePending)
            {
                renderDoc.TriggerCapture();
                capturePending = false;
            }
#endif

            var input = window.PumpEvents();
            if (!window.Exists)
                break;

            imguiRenderer.Update(1f / 60f, input);

            ImGui.SetNextWindowPos(new Vector2(0, 0));
            ImGui.SetNextWindowSize(new Vector2(window.Width, window.Height));
            ImGui.Begin("Correlator");

#if RENDERDOC
            ImGui.SameLine();
            if (ImGui.Button("RenderDoc Snapshot"))
                capturePending = true;

            ImGui.SameLine();
            if (ImGui.Button("Open RenderDoc"))
                renderDoc.LaunchReplayUI();
#endif
            correlator.Draw();

            ImGui.End();

            cl.Begin();
            cl.SetFramebuffer(gd.MainSwapchain.Framebuffer);
            cl.ClearColorTarget(0, RgbaFloat.Black);
            imguiRenderer.Render(gd, cl);
            cl.End();
            gd.SubmitCommands(cl);
            gd.SwapBuffers(gd.MainSwapchain);
        }
    }

    static void DumpFunctions(ProgramData program, string path, Func<GFunction, bool>? filter = null)
    {
        using var sw = new StreamWriter(path);
        foreach (var fn in program.Functions.OrderByDescending(x => x.Callees.Count + x.Callers.Count).Where(x => !x.IsIgnored))
            if (filter != null)
                sw.WriteLine(fn.ToString());
    }
}