using ImGuiNET;
using System;

namespace Resonant
{
    class ConfigurationUI : IDisposable, IDrawable
    {
        private ConfigurationManager configManager;
        private Configuration config;

        public ConfigurationUI(ConfigurationManager configManager, Configuration activeConfig)
        {
            this.configManager = configManager;
            this.config = activeConfig;
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        protected void ReadConfig()
        {

        }

        public void Draw()
        {
            if (!config.ConfigUIVisible) { return; }

            // FIXME: Always is a hack, ImGuiCond.FirstUseEver instead
            //ImGui.SetNextWindowSize(new Vector2(300, 500), ImGuiCond.Always);
            if (ImGui.Begin("Resonant Configuration", ref config.ConfigUIVisible))
            {
                if (ImGui.Button("Save"))
                {
                    this.configManager.Save(this.config);
                }
                ImGui.SameLine();
                if (ImGui.Button("Close"))
                {
                    config.ConfigUIVisible = false;
                }
                if (ImGui.Button("Configure Draw Window"))
                {
                    config.DrawUIVisible = !config.DrawUIVisible;
                }
                // TODO: cancel button that restores from pluginConfiguration
                // TODO: reset to defaults button

                ImGui.Separator();

                ImGui.Checkbox("Hitbox", ref config.Hitbox.Enabled);
                if (config.Hitbox.Enabled)
                {
                    ImGui.ColorEdit4("Hitbox Color", ref config.Hitbox.Color, ImGuiColorEditFlags.NoInputs);
                    ImGui.ColorEdit4("Outline Color", ref config.Hitbox.OutlineColor, ImGuiColorEditFlags.NoInputs);
                    ImGui.Checkbox("Show Target ΔY", ref config.Hitbox.ShowDeltaY);
                }

                ImGui.Separator();

                ImGui.Checkbox("Positionals", ref config.Positionals.Enabled);
                if (config.Positionals.Enabled)
                {
                    ImGui.Checkbox("Melee Ability Range (> Melee Range!)", ref config.Positionals.ShowAbilityRegions);
                    ImGui.Text("Thickness");
                    ImGui.DragInt("##Thickness", ref config.Positionals.Thickness, 1, 0, 50);
                    ImGui.ColorEdit4("Front Color", ref config.Positionals.ColorFront, ImGuiColorEditFlags.NoInputs);
                    ImGui.Checkbox("Separate Front Regions", ref config.Positionals.FrontSeparate);
                    ImGui.ColorEdit4("Flank Color", ref config.Positionals.ColorFlank, ImGuiColorEditFlags.NoInputs);
                    ImGui.ColorEdit4("Rear Color", ref config.Positionals.ColorRear, ImGuiColorEditFlags.NoInputs);
                    ImGui.Checkbox("Separate Rear Regions", ref config.Positionals.RearSeparate);
                    ImGui.Checkbox("Highlight Current Region", ref config.Positionals.HighlightCurrentRegion);
                    if (config.Positionals.HighlightCurrentRegion)
                    {
                        ImGui.Text("Highlight Alpha Multiplier");
                        ImGui.DragFloat("##Highlight Alpha", ref config.Positionals.HighlightTransparencyMultiplier, .01f, 0f, 1f);
                    }

                    ImGui.Checkbox("Front Arrow", ref config.Positionals.ArrowEnabled);
                    ImGui.DragFloat("Front Arrow Scale", ref config.Positionals.ArrowScale, .01f, 0f, 1f);
                }

                ImGui.Separator();

                ImGui.Checkbox("Player Ring", ref config.PlayerRing.Enabled);
                if (config.PlayerRing.Enabled)
                {
                    ImGui.DragFloat("Ring Yalms", ref config.PlayerRing.Radius, .25f, 1, 50);
                    ImGui.DragFloat("Ring Thickness", ref config.PlayerRing.Brush.Thickness, 1, 1, 50);
                    ImGui.ColorEdit4("Ring color", ref config.PlayerRing.Brush.Color, ImGuiColorEditFlags.NoInputs);
                }

                ImGui.Separator();

                ImGui.Checkbox("Target Ring", ref config.TargetRing.Enabled);
                if (config.TargetRing.Enabled)
                {
                    ImGui.DragFloat("Target Yalms", ref config.TargetRing.Radius, .25f, 1, 50);
                    ImGui.DragFloat("Target Thickness", ref config.TargetRing.Brush.Thickness, 1, 1, 50);
                    ImGui.ColorEdit4("Target color", ref config.TargetRing.Brush.Color, ImGuiColorEditFlags.NoInputs);
                }

                ImGui.Separator();

                ImGui.Checkbox("Cone", ref config.Cone.Enabled);
                if (config.Cone.Enabled)
                {
                    ImGui.DragFloat("Cone Yalms", ref config.Cone.Radius, .25f, 1, 50);
                    ImGui.DragInt("Cone Angle", ref config.Cone.Angle, 1, 1, 180);
                    ImGui.DragFloat("Cone Thickness", ref config.Cone.Brush.Thickness, 1, 1, 50);
                    ImGui.ColorEdit4("Cone Color", ref config.Cone.Brush.Color, ImGuiColorEditFlags.NoInputs);
                }

                ImGui.Separator();
                ImGui.Checkbox("Debug", ref config.DebugUIVisible);
            }
            ImGui.End();

            // FIXME: move
            this.DrawWindowBox();
        }

        void DrawWindowBox()
        {
            if (!config.DrawUIVisible)
            {
                return;
            }

            var displaySize = ImGui.GetIO().DisplaySize;
            var windowSize = config.WindowBox.SizeWith(displaySize);

            //ImGuiHelpers.ForceNextWindowMainViewport();
            //ImGuiHelpers.SetNextWindowPosRelativeMainViewport(config.WindowBox.TopLeft);
            ImGui.SetNextWindowPos(config.WindowBox.TopLeft, ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

            if (ImGui.Begin("Draw Window", ref config.DrawUIVisible))
            {
                ImGui.Text($"Drag and resize this window. Resonant will not draw outside of these bounds.");
                if (ImGui.Button($"Save and close"))
                {
                    this.configManager.Save(this.config);
                    config.DrawUIVisible = false;
                }
                if (ImGui.Button("Maximize"))
                {
                    config.WindowBox = new();
                    ImGui.SetWindowPos(config.WindowBox.TopLeft);
                }
                else
                {
                    config.WindowBox.TopLeft = ImGui.GetWindowPos();
                    config.WindowBox.BottomRight = displaySize - ImGui.GetWindowPos() - ImGui.GetWindowSize();
                }

                ImGui.Separator();

                ImGui.Text($"Settings");
                ImGui.Text($"Top left: {config.WindowBox.TopLeft}");
                ImGui.Text($"Bottom right: {config.WindowBox.BottomRight}");
                ImGui.Text($"Size: {config.WindowBox.SizeWith(displaySize)}");

                ImGui.Separator();

                ImGui.Text($"Display Size: {displaySize}");
                ImGui.Text($"Current window size: {ImGui.GetWindowSize()}");

            }
            ImGui.End();
        }
    }
}
