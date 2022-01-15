using ImGuiNET;
using System;
using System.Numerics;
using System.Text.RegularExpressions;

namespace Resonant
{
    class ConfigurationUI : IDisposable, IDrawable
    {
        private ConfigurationManager configManager;
        private ConfigurationProfile config;

        public ConfigurationUI(ConfigurationManager configManager, ConfigurationProfile activeConfig)
        {
            this.configManager = configManager;
            this.config = activeConfig;
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        public void Draw()
        {
            this.DrawMainConfig();
            this.DrawViewportConfigWindow();
        }

        void DrawMainConfig()
        {
            if (!config.ConfigUIVisible) { return; }

            ImGui.SetNextWindowSize(new Vector2(300, 500), ImGuiCond.FirstUseEver);
            ImGui.PushID("resonant");

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
                ImGui.SameLine();
                if (ImGui.Button("Configure Draw Window"))
                {
                    config.ViewportUIVisible = !config.ViewportUIVisible;
                }

                // todo: cancel button that restores from pluginConfiguration
                // todo: reset to defaults button

                ImGui.BeginTabBar("##Resonant Tabs");

                if (ImGui.BeginTabItem("Player"))
                {
                    ImGui.Checkbox("Hitbox", ref config.Hitbox.Enabled);
                    if (config.Hitbox.Enabled)
                    {
                        ImGui.ColorEdit4("Hitbox Color", ref config.Hitbox.Color, ImGuiColorEditFlags.NoInputs);
                        ImGui.ColorEdit4("Outline Color", ref config.Hitbox.OutlineColor, ImGuiColorEditFlags.NoInputs);
                        ImGui.Checkbox("Use Target Y", ref config.Hitbox.UseTargetY);
                        if (config.Hitbox.UseTargetY)
                        {
                            ImGui.Checkbox("Show Target ΔY", ref config.Hitbox.ShowTargetDeltaY);
                        }
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

                    ImGui.Checkbox("Cone", ref config.Cone.Enabled);
                    if (config.Cone.Enabled)
                    {
                        ImGui.DragFloat("Cone Yalms", ref config.Cone.Radius, .25f, 1, 50);
                        ImGui.DragInt("Cone Angle", ref config.Cone.Angle, 1, 1, 180);
                        ImGui.DragFloat("Cone Thickness", ref config.Cone.Brush.Thickness, 1, 1, 50);
                        ImGui.ColorEdit4("Cone Color", ref config.Cone.Brush.Color, ImGuiColorEditFlags.NoInputs);
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Target"))
                {

                    ImGui.Checkbox("Positionals", ref config.Positionals.Enabled);
                    if (config.Positionals.Enabled)
                    {
                        ImGui.Text("Thickness");
                        ImGui.DragInt("##Thickness", ref config.Positionals.Thickness, 1, 0, 50);

                        ImGui.Checkbox("Melee Ability Range (> Melee Range!)", ref config.Positionals.MeleeAbilityRange);
                        if (config.Positionals.MeleeAbilityRange)
                        {
                            ImGui.Text("Thickness");
                            ImGui.DragInt("##MeleeAbilityThickness", ref config.Positionals.MeleeAbilityThickness, 1, 1, 50);
                        }

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
                        if (config.Positionals.ArrowEnabled)
                        {
                            DragFloat("Arrow Scale##FrontArrow", ref config.Positionals.ArrowScale, .01f, 0f, 1f);
                        }
                    }

                    ImGui.Separator();

                    ImGui.Checkbox("Target Ring", ref config.TargetRing.Enabled);
                    if (config.TargetRing.Enabled)
                    {
                        ImGui.DragFloat("Target Yalms", ref config.TargetRing.Radius, .25f, 1, 50);
                        ImGui.DragFloat("Target Thickness", ref config.TargetRing.Brush.Thickness, 1, 1, 50);
                        ImGui.ColorEdit4("Target color", ref config.TargetRing.Brush.Color, ImGuiColorEditFlags.NoInputs);
                    }

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Debug"))
                {
                    ImGui.Checkbox("Debug UI", ref config.DebugUIVisible);

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.PopID();
            ImGui.End();
        }

        void DragFloat(string label, ref float v, float v_speed, float v_min, float v_max)
        {
            ImGui.Text(Regex.Replace(label, "##\\w+", ""));
            ImGui.DragFloat($"##{label}", ref v, v_speed, v_min, v_max);
        }

        void DrawViewportConfigWindow()
        {
            if (!config.ViewportUIVisible)
            {
                return;
            }

            var displaySize = ImGui.GetIO().DisplaySize;
            var windowSize = config.ViewportWindowBox.SizeWith(displaySize);

            ImGui.SetNextWindowPos(config.ViewportWindowBox.TopLeft, ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

            if (ImGui.Begin("Viewport Window", ref config.ViewportUIVisible))
            {
                ImGui.Text($"Drag and resize this window. Resonant will not draw outside of these bounds.");
                if (ImGui.Button($"Save and close"))
                {
                    this.configManager.Save(this.config);
                    config.ViewportUIVisible = false;
                }
                if (ImGui.Button("Maximize"))
                {
                    config.ViewportWindowBox = new();
                    ImGui.SetWindowPos(config.ViewportWindowBox.TopLeft);
                }
                else
                {
                    config.ViewportWindowBox.TopLeft = ImGui.GetWindowPos();
                    config.ViewportWindowBox.BottomRight = displaySize - ImGui.GetWindowPos() - ImGui.GetWindowSize();
                }

                ImGui.Separator();

                ImGui.Text($"Settings");
                ImGui.Text($"Top left: {config.ViewportWindowBox.TopLeft}");
                ImGui.Text($"Bottom right: {config.ViewportWindowBox.BottomRight}");
                ImGui.Text($"Size: {config.ViewportWindowBox.SizeWith(displaySize)}");

                ImGui.Separator();

                ImGui.Text($"Display Size: {displaySize}");
                ImGui.Text($"Current window size: {ImGui.GetWindowSize()}");

            }
            ImGui.End();
        }
    }
}
