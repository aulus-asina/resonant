using Dalamud.Game.Gui;
using Dalamud.Logging;
using ImGuiNET;
using System;
using System.Numerics;

namespace Resonant
{
    // makes complete shapes using coordinates from in-game space
    internal class ConvexShape
    {
        // number of segments used when rendering a circle
        private const int SEGMENTS = 100;

        Brush brush;
        GameGui gui;
        ImDrawListPtr draw;
        bool cullObject = true;

        internal ConvexShape(GameGui gui, Brush brush)
        {
            this.gui = gui;
            this.brush = brush;
            draw = ImGui.GetWindowDrawList();
            Initialize();
        }

        internal void Initialize()
        {
        }

        internal void Point(Vector3 worldPos)
        {
            // TODO: implement proper clipping. everything goes crazy when
            // drawing lines outside the clip window and behind the camera
            // point
            var visible = gui.WorldToScreen(worldPos, out Vector2 pos);
            draw.PathLineTo(pos);
            if (visible) { cullObject = false; }
        }

        internal void PointRadial(Vector3 center, float radius, float radians)
        {
            Point(new Vector3(
                center.X + (radius * (float)Math.Sin(radians)),
                center.Y,
                center.Z + (radius * (float)Math.Cos(radians))
            ));
        }

        internal void Arc(Vector3 center, float radius, float startRads, float endRads)
        {
            int segments = Maths.ArcSegments(startRads, endRads);
            var deltaRads = (endRads - startRads) / segments;

            for (var i = 0; i < segments + 1; i++)
            {
                PointRadial(center, radius, startRads + (deltaRads * i));
            }
        }

        internal void Done()
        {
            if (cullObject)
            {
                draw.PathClear();
                return;
            }

            if (brush.HasFill())
            {
                draw.PathFillConvex(ImGui.GetColorU32(brush.Fill));
            }
            else if (brush.Thickness != 0)
            {
                draw.PathStroke(ImGui.GetColorU32(brush.Color), ImDrawFlags.None, brush.Thickness);
            }
            draw.PathClear();
        }
    }
}
