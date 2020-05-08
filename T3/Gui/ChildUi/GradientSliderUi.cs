﻿using ImGuiNET;
using T3.Core.Logging;
using T3.Core.Operator;
using T3.Gui.UiHelpers;
using T3.Operators.Types.Id_8211249d_7a26_4ad0_8d84_56da72a5c536;
using UiHelpers;

namespace T3.Gui.ChildUi
{
    public static class GradientSliderUi
    {
        public static bool DrawChildUi(Instance instance, ImDrawListPtr drawList, ImRect selectableScreenRect)
        {
            if (!(instance is GradientSlider gradientSlider))
                return false;

            var innerRect = selectableScreenRect;
            innerRect.Expand(-4);

            var gradient = gradientSlider.Gradient.TypedDefaultValue.Value;
            if (gradient == null)
            {
                Log.Warning("Can't draw undefined gradient");
                return false;
            } 
            
            var modified = GradientEditor.Draw(gradient, drawList, innerRect);
            
            if( modified) 
                gradientSlider.Gradient.DirtyFlag.Invalidate();
            
            return modified;
        }
    }
}