﻿using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ImGuiNET;
using T3.Core.Logging;
using T3.Core.Resource;
using T3.Core.Utils;
using T3.Editor.Gui.Styling;
using T3.Editor.Gui.Windows;

namespace T3.Editor.Gui.UiHelpers
{
    /// <summary>
    /// Renders the <see cref="ConsoleLogWindow"/>
    /// </summary>
    public class StatusErrorLine : ILogWriter
    {
        public StatusErrorLine()
        {
            Log.AddWriter(this);
        }

        public void Draw()
        {
            lock (_logEntries)
            {
                if (_logEntries.Count == 0)
                {
                    ImGui.TextUnformatted("Log empty");
                    return;
                }

                var lastEntry = _logEntries[_logEntries.Count - 1];
                var color = ConsoleLogWindow.GetColorForLogLevel(lastEntry.Level)
                                            .Fade(MathUtils.RemapAndClamp((float)lastEntry.SecondsAgo, 0, 1.5f, 1, 0.4f));

                ImGui.PushFont(Fonts.FontBold);

                var logMessage = lastEntry.Message;
                if (lastEntry.Level == LogEntry.EntryLevel.Error)
                {
                    logMessage = ShaderResource.ExtractMeaningfulShaderErrorMessage(logMessage);
                }

                var width = ImGui.CalcTextSize(logMessage);
                var availableSpace = ImGui.GetWindowContentRegionMax().X;
                ImGui.SetCursorPosX(availableSpace - width.X);

                ImGui.TextColored(color, logMessage);
                if (ImGui.IsItemClicked())
                {
                    _logEntries.Clear();
                }
            }

            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                {
                    lock (_logEntries)
                    {
                        foreach (var entry in _logEntries)
                        {
                            ConsoleLogWindow.DrawEntry(entry);
                        }
                    }
                }
                ImGui.EndTooltip();
            }

            ImGui.PopFont();
        }

        public void Dispose()
        {
        }

        public LogEntry.EntryLevel Filter { get; set; }

        public void ProcessEntry(LogEntry entry)
        {
            lock (_logEntries)
            {
                if (_logEntries.Count > 20)
                {
                    _logEntries.RemoveAt(0);
                }

                _logEntries.Add(entry);
            }
        }

        private readonly List<LogEntry> _logEntries = new();
    }
}