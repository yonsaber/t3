﻿using System;
using ImGuiNET;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.CodeAnalysis.CSharp;
using T3.Core.Logging;
using T3.Core.Operator;
using T3.Gui.Graph.Interaction;
using T3.Gui.OutputUi;
using T3.Gui.UiHelpers;
using T3.Operators.Types;

namespace T3.Gui.Windows
{
    public class OutputWindow : Window
    {
        public OutputWindow()
        {
            Config.Title = "Output##" + _instanceCounter;
            Config.Visible = true;

            AllowMultipleInstances = true;
            Config.Visible = true;
            WindowFlags = ImGuiWindowFlags.NoScrollbar;

            _instanceCounter++;
            _outputWindowInstances.Add(this);
        }

        private static readonly List<Window> _outputWindowInstances = new List<Window>();

        protected override void DrawAllInstances()
        {
            // Wrap inside list to enable removable of members during iteration
            foreach (var w in _outputWindowInstances.ToList())
            {
                w.DrawOneInstance();
            }
        }

        protected override void Close()
        {
            _outputWindowInstances.Remove(this);
        }

        protected override void AddAnotherInstance()
        {
            new OutputWindow();
        }

        protected override void DrawContent()
        {
            ImGui.BeginChild("##content", new Vector2(0, ImGui.GetWindowHeight()), false, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove);
            {
                _imageCanvas.NoMouseInteraction = _selectedCamera != null;
                _imageCanvas.Update();

                ImGui.SetCursorPos(ImGui.GetWindowContentRegionMin() + new Vector2(0, 40));
                _cameraInteraction.Update(_selectedCamera);
                DrawSelection(_pinning.GetSelectedInstance());
                DrawToolbar();

                ImGui.SetCursorPos(new Vector2(0, 0));
            }
            ImGui.EndChild();
        }

        public override List<Window> GetInstances()
        {
            return _outputWindowInstances;
        }

        private Camera[] FindCameras()
        {
            var instance = _pinning.GetSelectedInstance();
            if (instance == null)
                return new Camera[] { };
            
            return instance.Parent?.Children.OfType<Camera>().ToArray();
        }

        
        // private Camera FindCameraInstance()
        // {
        //     if (_pinning.SelectedInstance.Parent == null)
        //         return null;
        //
        //     var obj = _pinning.SelectedInstance.Parent.Children.FirstOrDefault(child => child.Type == typeof(Camera));
        //     var cam = obj as Camera;
        //     return cam;
        // }

        private void DrawToolbar()
        {
            ImGui.SetCursorPos(ImGui.GetWindowContentRegionMin());
            _pinning.DrawPinning();

            if (ImGui.Button("1:1"))
            {
                _imageCanvas.SetScaleToMatchPixels();
                _imageCanvas.SetViewMode(ImageOutputCanvas.Modes.Pixel);
            }

            ImGui.SameLine();

            if (ImGui.Button("M"))
            {
                _imageCanvas.SetViewMode(ImageOutputCanvas.Modes.Fitted);
            }

            ImGui.SameLine();

            DrawCameraSelection();
        }

        private void DrawCameraSelection()
        {
            var cameras = FindCameras();
            if (cameras == null || cameras.Length==0)
                return;
            
            _selectedCamera = cameras.FirstOrDefault(cam => cam.SymbolChildId == _selectedCameraId);
            if (_selectedCamera == null)
            {
                _selectedCamera = cameras.First();
                _selectedCameraId = _selectedCamera.SymbolChildId;
            }
            else if (_selectedCameraId == Guid.Empty)
            {
                _selectedCameraId = cameras.First().SymbolChildId;
            }

            ImGui.SetNextItemWidth(100);
            if (ImGui.BeginCombo("##CameraSelection", _selectedCamera.Symbol.Name))
            {
                foreach (var cam in FindCameras())
                {
                    ImGui.PushID(cam.SymbolChildId.GetHashCode());
                    {
                        var instance = _pinning.GetSelectedInstance();
                        var symbolChild = SymbolRegistry.Entries[instance.Parent.Symbol.Id].Children.Single(child => child.Id == cam.SymbolChildId);
                        ImGui.Selectable(symbolChild.ReadableName, cam == _selectedCamera);
                        if (ImGui.IsItemActivated())
                        {
                            _selectedCameraId = cam.SymbolChildId;
                        }

                        if (ImGui.IsItemHovered())
                        {
                            T3Ui.AddHoveredId(cam.SymbolChildId);
                        }
                    }
                    ImGui.PopID();
                }
            }
        }


        private static void DrawSelection(Instance instance)
        {
            if (instance == null)
                return;

            if (instance.Outputs.Count <= 0)
                return;

            var symbolUi = SymbolUiRegistry.Entries[instance.Symbol.Id];
            
            var firstOutput = instance.Outputs[0];
            if (!symbolUi.OutputUis.ContainsKey(firstOutput.Id))
                return;

            IOutputUi outputUi = symbolUi.OutputUis[firstOutput.Id];
            outputUi.DrawValue(firstOutput);
        }

        private readonly ImageOutputCanvas _imageCanvas = new ImageOutputCanvas();
        private readonly SelectionPinning _pinning = new SelectionPinning();
        private readonly CameraInteraction _cameraInteraction = new CameraInteraction();
        
        private Guid _selectedCameraId = Guid.Empty;
        static int _instanceCounter;
        private Camera _selectedCamera;
    }
}