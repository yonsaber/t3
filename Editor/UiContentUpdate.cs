﻿using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using ImGuiNET;
using SharpDX.Direct3D11;
using T3.Core.Compilation;
using T3.Core.Logging;
using T3.Core.Operator.Slots;
using T3.Core.Resource;
using T3.Editor.App;
using T3.Editor.Compilation;
using T3.Editor.Gui;
using T3.Editor.Gui.Styling;
using T3.Editor.Gui.UiHelpers;
using T3.Editor.Gui.Windows;

namespace T3.Editor;

internal static class UiContentUpdate
{
    public static void RenderCallback()
    {
        var cursorPos = Cursor.Position;
        CursorPosOnScreen = new Vector2(cursorPos.X, cursorPos.Y);
        IsCursorInsideAppWindow = ProgramWindows.Main.IsCursorOverWindow;

        // Update font atlas texture if UI-Scale changed
        if (Math.Abs(UserSettings.Config.UiScaleFactor - _lastUiScale) > 0.005f)
        {
            GenerateFontsWithScaleFactor(UserSettings.Config.UiScaleFactor);
            _lastUiScale = UserSettings.Config.UiScaleFactor;
        }

        if (ProgramWindows.Main.IsMinimized && UserSettings.Config.SuspendRenderingWhenHidden)
        {
            Thread.Sleep(100);
            return;
        }

        Int64 ticks = _stopwatch.ElapsedTicks;
        Int64 ticksDiff = ticks - _lastElapsedTicks;
        ImGui.GetIO().DeltaTime = (float)((double)(ticksDiff) / Stopwatch.Frequency);
        _lastElapsedTicks = ticks;
        ImGui.GetIO().DisplaySize = ProgramWindows.Main.Size;

        ProgramWindows.HandleFullscreenToggle();
        OperatorUpdating.UpdateChangedOperators();

        DirtyFlag.IncrementGlobalTicks();
        T3Metrics.UiRenderingStarted();

        if (!string.IsNullOrEmpty(Program.RequestImGuiLayoutUpdate))
        {
            ImGui.LoadIniSettingsFromMemory(Program.RequestImGuiLayoutUpdate);
            Program.RequestImGuiLayoutUpdate = null;
        }

        ImGui.NewFrame();
        ProgramWindows.Main.PrepareRenderingFrame();

        // Render 2nd view
        ProgramWindows.Viewer.SetVisible(T3Ui.ShowSecondaryRenderWindow);

        if (T3Ui.ShowSecondaryRenderWindow)
        {
            var viewer = ProgramWindows.Viewer;
            ProgramWindows.Viewer.PrepareRenderingFrame();

            ProgramWindows.SetVertexShader(SharedResources.FullScreenVertexShaderResource);
            ProgramWindows.SetPixelShader(SharedResources.FullScreenPixelShaderResource);

            if (viewer.Texture is { IsDisposed: false })
            {
                //Log.Debug($"using TextureId:{resourceManager.SecondRenderWindowTexture}, debug name:{resourceManager.SecondRenderWindowTexture.DebugName}");
                if (_viewWindowBackgroundSrv == null ||
                    _viewWindowBackgroundSrv.Resource.NativePointer != viewer.Texture.NativePointer)
                {
                    _viewWindowBackgroundSrv?.Dispose();
                    _viewWindowBackgroundSrv = new ShaderResourceView(Program.Device, viewer.Texture);
                }

                ProgramWindows.SetRasterizerState(SharedResources.ViewWindowRasterizerState);
                ProgramWindows.SetPixelShaderSRV(_viewWindowBackgroundSrv);
            }
            else if (ResourceManager.ResourcesById[SharedResources.ViewWindowDefaultSrvId] is ShaderResourceViewResource srvr)
            {
                ProgramWindows.SetPixelShaderSRV(srvr.ShaderResourceView);
            }
            else
            {
                Log.Debug($"Invalid {nameof(ShaderResourceView)} for 2nd render view");
            }
    
            ProgramWindows.CopyToSecondaryRenderOutput();
        }

        T3Ui.ProcessFrame();

        ProgramWindows.RefreshViewport();

        ImGui.Render();
        Program.UiContentContentDrawer.RenderDrawData(ImGui.GetDrawData());

        T3Metrics.UiRenderingCompleted();

        ProgramWindows.Present(T3Ui.UseVSync, T3Ui.ShowSecondaryRenderWindow);
    }

    public static void GenerateFontsWithScaleFactor(float scaleFactor)
    {
        // See https://stackoverflow.com/a/5977638
        T3Ui.DisplayScaleFactor = ProgramWindows.Main.GetDpi().X / 96f;
        var dpiAwareScale = scaleFactor * T3Ui.DisplayScaleFactor;

        T3Ui.UiScaleFactor = dpiAwareScale;

        var fontAtlasPtr = ImGui.GetIO().Fonts;
        fontAtlasPtr.Clear();
        var root = Path.Combine(RuntimeAssemblies.CorePath, "Resources", "t3-editor", "fonts");
        Fonts.FontNormal = fontAtlasPtr.AddFontFromFileTTF(Path.Combine(root, "Roboto-Regular.ttf"), 18f * dpiAwareScale);
        Fonts.FontBold = fontAtlasPtr.AddFontFromFileTTF(Path.Combine(root, "Roboto-Medium.ttf"), 18f * dpiAwareScale);
        Fonts.FontSmall = fontAtlasPtr.AddFontFromFileTTF(Path.Combine(root, "Roboto-Regular.ttf"), 14f * dpiAwareScale);
        Fonts.FontLarge = fontAtlasPtr.AddFontFromFileTTF(Path.Combine(root, "Roboto-Light.ttf"), 30f * dpiAwareScale);

        Program.UiContentContentDrawer.CreateDeviceObjects();
    }
    
    private static long _lastElapsedTicks;
    private static readonly Stopwatch _stopwatch = new() ;
    
    private static float _lastUiScale = 1;
    private static ShaderResourceView _viewWindowBackgroundSrv;
    public static Vector2 CursorPosOnScreen { get; private set; }
    public static bool IsCursorInsideAppWindow { get; private set; }

    public static void StartMeasureFrame()
    {
        _stopwatch.Start();
        _lastElapsedTicks = _stopwatch.ElapsedTicks;
    }
}