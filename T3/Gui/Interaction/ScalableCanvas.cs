﻿using System;
using System.Numerics;
using ImGuiNET;
using T3.Core;
using T3.Gui.Graph;
using T3.Gui.Styling;
using T3.Gui.UiHelpers;
using T3.Gui.Windows.TimeLine;
using UiHelpers;

namespace T3.Gui.Interaction
{
    /// <summary>
    /// Implements transformations and interactions for a canvas that can
    /// be zoomed and panned.
    /// </summary>
    public class ScalableCanvas : ICanvas
    {
        /// <summary>
        /// This needs to be called by the inherited class before drawing its interface. 
        /// </summary>
        public void UpdateCanvas(T3Ui.EditingFlags flags = T3Ui.EditingFlags.None)
        {
            Io = ImGui.GetIO();
            _mouse = ImGui.GetMousePos();

            if (FillMode == FillModes.FillWindow)
            {
                WindowPos = ImGui.GetWindowContentRegionMin() + ImGui.GetWindowPos() + new Vector2(1, 1);
                WindowSize = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin() - new Vector2(2, 2);
            }
            else
            {
                WindowSize = ImGui.GetContentRegionAvail();
                WindowPos = ImGui.GetCursorScreenPos();
            }

            if (!UsingParentCanvas)
                DampScaling();

            HandleInteraction(flags);
        }

        #region implement ICanvas =================================================================
        /// <summary>
        /// Convert canvas position (e.g. of an Operator) into screen position  
        /// </summary>
        public virtual Vector2 TransformPosition(Vector2 posOnCanvas)
        {
            var v = posOnCanvas * Scale - Scroll * Scale + WindowPos;
            return new Vector2((int)v.X, (int)v.Y);
        }

        public virtual Vector2 TransformPositionFloat(Vector2 posOnCanvas)
        {
            var v = posOnCanvas * Scale - Scroll * Scale + WindowPos;
            return new Vector2(v.X, v.Y);
        }

        public Vector2 TransformPositionFloored(Vector2 posOnCanvas)
        {
            return MathUtils.Floor(posOnCanvas * Scale - Scroll * Scale + WindowPos);
        }

        /// <summary>
        /// Convert canvas position (e.g. of an Operator) to screen position  
        /// </summary>
        public float TransformX(float xOnCanvas)
        {
            return TransformPosition(new Vector2(xOnCanvas, 0)).X;
        }

        /// <summary>
        ///  Convert canvas position (e.g. of an Operator) to screen position 
        /// </summary>
        public float TransformY(float yOnCanvas)
        {
            return TransformPosition(new Vector2(0, yOnCanvas)).Y;
        }

        /// <summary>
        /// Convert a screen space position (e.g. from mouse) to canvas coordinates  
        /// </summary>
        public virtual Vector2 InverseTransformPosition(Vector2 screenPos)
        {
            return (screenPos - WindowPos) / Scale + Scroll;
        }

        /// <summary>
        /// Convert screen position to canvas position
        /// </summary>
        public virtual float InverseTransformX(float xOnScreen)
        {
            return InverseTransformPosition(new Vector2(xOnScreen, 0)).X;
        }

        /// <summary>
        /// Convert screen position to canvas position
        /// </summary>
        public float InverseTransformY(float yOnScreen)
        {
            //return (yOnScreen - WindowPos.Y) / Scale.Y + Scroll.Y;
            //return (yOnScreen - WindowPos.Y - WindowPos.Y) / Scale.Y;
            return InverseTransformPosition(new Vector2(yOnScreen, 0)).Y;
        }

        /// <summary>
        /// Convert a direction (e.g. MouseDelta) from ScreenSpace to Canvas
        /// </summary>
        public Vector2 TransformDirection(Vector2 vectorInCanvas)
        {
            return vectorInCanvas * Scale;
        }

        /// <summary>
        /// Convert a direction (e.g. MouseDelta) from ScreenSpace to Canvas
        /// </summary>
        public Vector2 InverseTransformDirection(Vector2 vectorInScreen)
        {
            return vectorInScreen / Scale;
        }

        public ImRect TransformRect(ImRect canvasRect)
        {
            // NOTE: We have to floor the size instead to min max position to avoid jittering  
            var min = TransformPositionFloat(canvasRect.Min);
            var max = TransformPositionFloat(canvasRect.Max);
            var size = max - min;
            min.X = (int)min.X;
            min.Y = (int)min.Y;
            size.X = (int)size.X;
            size.Y = (int)size.Y;
            return new ImRect(min, min + size);
        }

        public ImRect InverseTransformRect(ImRect screenRect)
        {
            return new ImRect(InverseTransformPosition(screenRect.Min), InverseTransformPosition(screenRect.Max));
        }

        /// <summary>
        /// Transform a canvas position to relative position within ImGui-window (e.g. to set ImGui context) 
        /// </summary>
        public Vector2 ChildPosFromCanvas(Vector2 posOnCanvas)
        {
            return posOnCanvas * Scale - Scroll * Scale;
        }

        public Vector2 WindowPos { get; private set; }
        public Vector2 WindowSize { get; private set; }

        public Vector2 Scale { get; private set; } = Vector2.One;
        protected Vector2 ScaleTarget = Vector2.One;

        public Vector2 Scroll { get; private set; } = new Vector2(0.0f, 0.0f);
        protected Vector2 ScrollTarget = new Vector2(0.0f, 0.0f);
        #endregion

        public Scope GetTargetScope()
        {
            return new Scope()
                       {
                           Scale = ScaleTarget,
                           Scroll = ScrollTarget
                       };
        }

        public void SetVisibleRange(Vector2 scale, Vector2 scroll)
        {
            ScaleTarget = scale;
            ScrollTarget = scroll;
        }

        public void SetVisibleVRange(float valueScale, float valueScroll)
        {
            ScaleTarget = new Vector2(ScaleTarget.X, valueScale);
            ScrollTarget = new Vector2(ScrollTarget.X, valueScroll);
        }

        public void SetScaleToMatchPixels()
        {
            ScaleTarget = Vector2.One;
        }

        /// <summary>
        /// This is a horrible hack: We sacrifice smooth interpolated scaling so we can use...
        /// - ... ScaleTarget for the local view
        /// - ... Scale for the view transformed to the parent canvas
        /// </summary>
        public void SetScaleToParentCanvas(ScalableCanvas parent)
        {
            if (parent == null)
                return;

            Scale = ScaleTarget * parent.Scale;
            Scroll = ScrollTarget;
        }

        public void SetScopeToCanvasArea(ImRect area, bool flipY = false, ScalableCanvas parent = null)
        {
            var areaSize = area.GetSize();
            if (areaSize.X == 0)
                areaSize.X = 1;

            if (areaSize.Y == 0)
                areaSize.Y = 1;

            if (Scale.X == 0 || Scale.Y == 0)
            {
                Scale = Vector2.One;
            }

            ScaleTarget = WindowSize / areaSize;

            if (flipY)
            {
                ScaleTarget.Y *= -1;
            }

            if (parent != null)
            {
                ScaleTarget /= parent.Scale;
            }

            ScrollTarget = new Vector2(area.Min.X,
                                       area.Max.Y);
        }

        public void SetVerticalScopeToCanvasArea(ImRect area, bool flipY = false, ScalableCanvas parent = null)
        {
            WindowSize = ImGui.GetContentRegionMax() - ImGui.GetWindowContentRegionMin();
            ScaleTarget.Y = WindowSize.Y / area.GetSize().Y;

            if (flipY)
            {
                ScaleTarget.Y *= -1;
            }

            if (parent != null)
            {
                ScaleTarget.Y /= parent.Scale.Y;
            }

            ScrollTarget.Y = area.Max.Y;
        }

        public void FitAreaOnCanvas(ImRect areaOnCanvas, bool flipY = false)
        {
            var heightOnCanvas = areaOnCanvas.GetHeight();
            var widthOnCanvas = areaOnCanvas.GetWidth();
            var aspectOnCanvas = widthOnCanvas / heightOnCanvas;

            // Use a fallback resolution to fix initial call from constructor
            // where img has not been initialized yet.
            if (WindowSize == Vector2.Zero)
            {
                WindowSize = new Vector2(800, 500);
            }

            float scale;
            if (aspectOnCanvas > WindowSize.X / WindowSize.Y)
            {
                // Center in a high window...
                scale = WindowSize.X / widthOnCanvas;
                ScrollTarget = new Vector2(
                                           areaOnCanvas.Min.X,
                                           areaOnCanvas.Min.Y - (WindowSize.Y / scale - heightOnCanvas) / 2);
            }
            else
            {
                // Center in a wide window... 
                scale = WindowSize.Y / heightOnCanvas;
                ScrollTarget = new Vector2(
                                           areaOnCanvas.Min.X - (WindowSize.X / scale - widthOnCanvas) / 2,
                                           areaOnCanvas.Min.Y);
            }

            ScaleTarget = new Vector2(scale, scale);
            if (flipY)
            {
                ScaleTarget.Y *= -1;
            }
        }

        public enum Transition
        {
            JumpIn,
            JumpOut,
            Undefined,
        }

        protected void SetScopeWithTransition(Vector2 scale, Vector2 scroll, Vector2 previousFocusOnScreen, Transition transition)
        {
            if (float.IsInfinity(scale.X) || float.IsNaN(scale.X)
                                          || float.IsInfinity(scale.Y) || float.IsNaN(scale.Y)
                                          || float.IsInfinity(scroll.X) || float.IsNaN(scroll.X)
                                          || float.IsInfinity(scroll.Y) || float.IsNaN(scroll.Y)
                )
            {
                scale = Vector2.One;
                scroll = Vector2.Zero;
            }

            ScaleTarget = scale;
            ScrollTarget = scroll;

            switch (transition)
            {
                case Transition.JumpIn:
                    Scale = ScaleTarget * 0.3f;
                    var sizeOnCanvas = WindowSize / Scale;
                    Scroll = ScrollTarget - sizeOnCanvas / 2;
                    break;

                case Transition.JumpOut:
                    Scale = ScaleTarget * 3f;
                    var sizeOnCanvas2 = WindowSize / Scale;
                    Scroll = ScrollTarget + sizeOnCanvas2 / 2;

                    break;
                default:
                    Scroll = ScaleTarget;
                    Scroll = ScrollTarget;
                    break;
            }
        }

        protected void DampScaling()
        {
            // Damp scaling
            var minInCanvas = Scroll;
            var maxInCanvas = Scroll + WindowSize / Scale;
            var minTargetInCanvas = ScrollTarget;
            var maxTargetInCanvas = ScrollTarget + WindowSize / ScaleTarget;

            var f = Math.Min(Io.DeltaTime / UserSettings.Config.ScrollSmoothing.Clamp(0.01f, 0.99f), 1);

            var min = Vector2.Lerp(minInCanvas, minTargetInCanvas, f);
            var max = Vector2.Lerp(maxInCanvas, maxTargetInCanvas, f);
            Scale = WindowSize / (max - min);
            Scroll = min;

            var completed = Math.Abs(Scroll.X - ScrollTarget.X) < 1f
                            && Math.Abs(Scroll.Y - ScrollTarget.Y) < 1f
                            && Math.Abs(Scale.X - ScaleTarget.X) < 0.05f
                            && Math.Abs(Scale.Y - ScaleTarget.Y) < 0.05f;

            if (completed)
            {
                Scroll = ScrollTarget;
                Scale = ScaleTarget;
            }

            if (float.IsNaN(ScaleTarget.X))
                ScaleTarget.X = 1;

            if (float.IsNaN(ScaleTarget.Y))
                ScaleTarget.Y = 1;

            if (float.IsNaN(Scale.X) || float.IsNaN(Scale.Y))
                Scale = ScaleTarget;

            if (float.IsNaN(ScrollTarget.X))
                ScrollTarget.X = 0;

            if (float.IsNaN(ScrollTarget.Y))
                ScrollTarget.Y = 0;

            if (float.IsNaN(Scroll.X) || float.IsNaN(Scroll.Y))
                Scroll = ScrollTarget;
        }

        protected virtual void HandleInteraction(T3Ui.EditingFlags flags)
        {
            var isDraggingConnection = (ConnectionMaker.TempConnections.Count > 0) && ImGui.IsWindowFocused();

            if (!ImGui.IsWindowHovered() && !isDraggingConnection)
                return;

            if (PreventMouseInteraction)
                return;

            if ((flags & T3Ui.EditingFlags.PreventPanningWithMouse) == 0
                && (
                       ImGui.IsMouseDragging(ImGuiMouseButton.Right)
                       || ImGui.IsMouseDragging(ImGuiMouseButton.Left) && ImGui.GetIO().KeyAlt)
                )
            {
                ScrollTarget -= Io.MouseDelta / (ParentScale * ScaleTarget);
                UserScrolledCanvas = true;
            }
            else
            {
                UserScrolledCanvas = false;
            }

            if ((flags & T3Ui.EditingFlags.PreventZoomWithMouseWheel) == 0)
            {
                ZoomWithMouseWheel();
                //ZoomWithMiddleMouseDrag();

                ScaleTarget = ClampScaleToValidRange(ScaleTarget);
            }
        }

        private Vector2 ClampScaleToValidRange(Vector2 scale)
        {
            if (IsCurveCanvas)
                return scale;

            return this is TimeLineCanvas
                       ? new Vector2(scale.X.Clamp(0.01f, 5000), scale.Y.Clamp(0.01f, 5000))
                       : new Vector2(scale.X.Clamp(0.1f, 11), scale.Y.Clamp(0.1f, 11));
        }

        private void ZoomWithMouseWheel()
        {
            UserZoomedCanvas = false;

            var focusCenterOnCanvas = InverseTransformPosition(_mouse);
            //DrawCanvasDebugInfos();

            var zoomDelta = ComputeZoomDeltaFromMouseWheel();
            var clamped = ClampScaleToValidRange(ScaleTarget * zoomDelta);
            if (clamped == ScaleTarget)
                return;

            var cornerOnCanvas = ScrollTarget;
            var cornerToFocus = focusCenterOnCanvas - cornerOnCanvas;

            if (Math.Abs(zoomDelta - 1) < 0.001f)
                return;

            if (IsCurveCanvas)
            {
                if (ImGui.GetIO().KeyAlt)
                {
                    ScaleTarget.X *= zoomDelta;
                }
                else if (ImGui.GetIO().KeyShift)
                {
                    ScaleTarget.Y *= zoomDelta;
                }
                else
                {
                    ScaleTarget *= zoomDelta;
                }
            }
            else
            {
                ScaleTarget *= zoomDelta;
            }

            if (Math.Abs(zoomDelta) > 0.1f)
                UserZoomedCanvas = true;

            //var shift = ScrollTarget * ScaleTarget + (focusCenter * ScaleTarget / parentZoom);
            ScrollTarget = (focusCenterOnCanvas - cornerToFocus / zoomDelta);
        }

        private void DrawCanvasDebugInfos()
        {
            var focusCenterOnCanvas = InverseTransformPosition(_mouse);
            var dl = ImGui.GetForegroundDrawList();

            var focusOnScreen = TransformPosition(focusCenterOnCanvas);
            dl.AddCircle(focusOnScreen, 30, Color.Green);
            dl.AddText(focusOnScreen + new Vector2(0, 0), Color.Orange, $"{focusCenterOnCanvas.X:0.0} {focusCenterOnCanvas.Y:0.0} ");

            var wp = ImGui.GetWindowPos();
            dl.AddRectFilled(wp, wp + new Vector2(200, 100), Color.Black.Fade(0.4f));
            dl.AddText(wp + new Vector2(0, 0), Color.Orange, $"SCAL: {ScaleTarget.X:0.0} {ScaleTarget.Y:0.0} ");
            dl.AddText(wp + new Vector2(0, 16), Color.Orange, $"SCRL: {ScrollTarget.X:0.0} {ScrollTarget.Y:0.0} ");
            dl.AddText(wp + new Vector2(0, 32), Color.Orange, $"CNVS: {focusCenterOnCanvas.X:0.0} {focusCenterOnCanvas.Y:0.0} ");
        }

        private bool IsCurveCanvas => Scale.Y < 0;

        private float ComputeZoomDeltaFromMouseWheel()
        {
            const float zoomSpeed = 1.2f;
            var zoomSum = 1f;
            var ioMouseWheel = Io.MouseWheel;

            if (ioMouseWheel < 0.0f)
            {
                for (var zoom = ioMouseWheel; zoom < 0.0f; zoom += 1.0f)
                {
                    zoomSum /= zoomSpeed;
                }
            }

            if (ioMouseWheel > 0.0f)
            {
                for (var zoom = ioMouseWheel; zoom > 0.0f; zoom -= 1.0f)
                {
                    zoomSum *= zoomSpeed;
                }
            }

            zoomSum = zoomSum.Clamp(0.02f, 100f);
            return zoomSum;
        }

        // private void ZoomWithMiddleMouseDrag()
        // {
        //     if (ImGui.IsMouseClicked(ImGuiMouseButton.Middle))
        //     {
        //         _mousePosWhenMiddlePressed = ImGui.GetMousePos();
        //         _scaleWhenMiddlePressed = ScaleTarget;
        //     }
        //
        //     if (ImGui.IsMouseDragging(ImGuiMouseButton.Middle, 0))
        //     {
        //         var delta = ImGui.GetMousePos() - _mousePosWhenMiddlePressed;
        //         var deltaMax = Math.Abs(delta.X) > Math.Abs(delta.Y)
        //                            ? -delta.X
        //                            : delta.Y;
        //         if (IsCurveCanvas)
        //         {
        //         }
        //         else
        //         {
        //             var f = (float)Math.Pow(1.1f, -deltaMax / 40f);
        //             ScaleTarget = _scaleWhenMiddlePressed * f;
        //             var focusCenter = (_mousePosWhenMiddlePressed - Scroll*Scale - WindowPos) / Scale; // ????????
        //             var shift = ScrollTarget * ScaleTarget + (focusCenter * ScaleTarget);
        //             ScrollTarget -= (_mousePosWhenMiddlePressed - shift - WindowPos) * ScaleTarget;
        //         }
        //     }
        // }

        private bool UsingParentCanvas => GraphCanvas.Current != this && GraphCanvas.Current != null;
        private Vector2 ParentScale => UsingParentCanvas ? GraphCanvas.Current.ScaleTarget : Vector2.One;

        public struct Scope
        {
            public Vector2 Scale;
            public Vector2 Scroll;
        }

        public enum FillModes
        {
            FillWindow,
            FillAvailableContentRegion,
        }

        public FillModes FillMode = FillModes.FillWindow;

        protected bool UserZoomedCanvas;
        protected bool UserScrolledCanvas;
        public bool PreventMouseInteraction;
        private Vector2 _mouse;
        protected ImGuiIOPtr Io;
    }
}