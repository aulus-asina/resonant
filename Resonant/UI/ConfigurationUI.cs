using ImGuiNET;
using System;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Dalamud.Data;
using Lumina.Excel.GeneratedSheets;

namespace Resonant
{
    class ConfigurationUI : IDrawable
    {
        ConfigurationManager ConfigManager { get; }
        Configuration Config { get { return ConfigManager.Config; } }
        ConfigurationProfile Profile { get { return ConfigManager.ActiveProfile; } }
        List<ConfigurationProfile> Profiles { get { return ConfigManager.Config.Profiles; } }

        DataManager DataManager { get; }

        private byte[] PromptProfileName = new byte[512];

        public ConfigurationUI(ConfigurationManager configManager, DataManager dataManager)
        {
            ConfigManager = configManager;
            DataManager = dataManager;
        }

        public void Draw()
        {
            ImGui.PushID("resonant_config");
            DrawMainConfig();
            DrawViewportConfigWindow();
            ImGui.PopID();
        }

        void DrawMainConfig()
        {
            if (!ConfigManager.ConfigUIVisible) { return; }

            ImGui.SetNextWindowSize(new Vector2(300, 500), ImGuiCond.FirstUseEver);

            if (ImGui.Begin("Resonant Configuration", ref ConfigManager.ConfigUIVisible))
            {
                if (ImGui.Button("Save"))
                {
                    ConfigManager.Save();
                }
                ImGui.SameLine();
                if (ImGui.Button("Close"))
                {
                    ConfigManager.ConfigUIVisible = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("Configure Draw Window"))
                {
                    ConfigManager.ViewportUIVisible = !ConfigManager.ViewportUIVisible;
                }

                // todo: cancel button that restores from pluginConfiguration
                // todo: reset to defaults button

                ImGui.BeginTabBar("##tabs");


                if (ImGui.BeginTabItem("Player"))
                {
                    TabPlayer();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Target"))
                {
                    TabTarget();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Profile"))
                {
                    TabProfile();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Debug"))
                {
                    TabDebug();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.End();
        }

        void TabPlayer()
        {
            ImGui.Checkbox("Hitbox", ref Profile.Hitbox.Enabled);
            if (Profile.Hitbox.Enabled)
            {
                ImGui.ColorEdit4("Hitbox Color", ref Profile.Hitbox.Color, ImGuiColorEditFlags.NoInputs);
                ImGui.ColorEdit4("Outline Color", ref Profile.Hitbox.OutlineColor, ImGuiColorEditFlags.NoInputs);
                ImGui.Checkbox("Use Target Y", ref Profile.Hitbox.UseTargetY);
                if (Profile.Hitbox.UseTargetY)
                {
                    ImGui.Checkbox("Show Target ΔY", ref Profile.Hitbox.ShowTargetDeltaY);
                }
            }

            ImGui.Separator();

            ImGui.Checkbox("Player Ring", ref Profile.PlayerRing.Enabled);
            if (Profile.PlayerRing.Enabled)
            {
                DragFloat("Ring Yalms", ref Profile.PlayerRing.Radius, .25f, 1, 50);
                DragFloat("Ring Thickness", ref Profile.PlayerRing.Brush.Thickness, 1, 1, 50);
                ImGui.ColorEdit4("Ring color", ref Profile.PlayerRing.Brush.Color, ImGuiColorEditFlags.NoInputs);
            }

            ImGui.Separator();

            ImGui.Checkbox("Cone", ref Profile.Cone.Enabled);
            if (Profile.Cone.Enabled)
            {
                DragFloat("Cone Yalms", ref Profile.Cone.Radius, .25f, 1, 50);
                DragInt("Cone Angle", ref Profile.Cone.Angle, 1, 1, 180);
                DragFloat("Cone Thickness", ref Profile.Cone.Brush.Thickness, 1, 1, 50);
                ImGui.ColorEdit4("Cone Color", ref Profile.Cone.Brush.Color, ImGuiColorEditFlags.NoInputs);
            }
        }

        void TabTarget()
        {
            ImGui.Checkbox("Positionals", ref Profile.Positionals.Enabled);
            if (Profile.Positionals.Enabled)
            {
                DragInt("Thickness", ref Profile.Positionals.Thickness, 1, 0, 50);

                ImGui.Checkbox("Melee Ability Range (> Melee Range!)", ref Profile.Positionals.MeleeAbilityRange);
                if (Profile.Positionals.MeleeAbilityRange)
                {
                    DragInt("Thickness##MeleeAbilityThickness", ref Profile.Positionals.MeleeAbilityThickness, 1, 1, 50);
                }

                ImGui.ColorEdit4("Front Color", ref Profile.Positionals.ColorFront, ImGuiColorEditFlags.NoInputs);
                ImGui.Checkbox("Separate Front Regions", ref Profile.Positionals.FrontSeparate);

                ImGui.ColorEdit4("Flank Color", ref Profile.Positionals.ColorFlank, ImGuiColorEditFlags.NoInputs);
                if (ImGui.BeginCombo("Flank Regions", Profile.Positionals.FlankType.Description()))
                {
                    foreach (FlankRegionSetting setting in Enum.GetValues(typeof(FlankRegionSetting)))
                    {
                        if (ImGui.Selectable(setting.Description()))
                        {
                            Profile.Positionals.FlankType = setting;
                        }
                    }
                    ImGui.EndCombo();
                }

                ImGui.ColorEdit4("Rear Color", ref Profile.Positionals.ColorRear, ImGuiColorEditFlags.NoInputs);
                ImGui.Checkbox("Separate Rear Regions", ref Profile.Positionals.RearSeparate);

                ImGui.Checkbox("Highlight Current Region", ref Profile.Positionals.HighlightCurrentRegion);
                if (Profile.Positionals.HighlightCurrentRegion)
                {
                    DragFloat("Highlight Alpha Multiplier", ref Profile.Positionals.HighlightTransparencyMultiplier, .01f, 0f, 1f);
                }

                ImGui.Checkbox("Front Arrow", ref Profile.Positionals.ArrowEnabled);
                if (Profile.Positionals.ArrowEnabled)
                {
                    DragFloat("Arrow Scale##FrontArrow", ref Profile.Positionals.ArrowScale, .01f, 0f, 1f);
                }
            }

            ImGui.Separator();

            ImGui.Checkbox("Target Ring", ref Profile.TargetRing.Enabled);
            if (Profile.TargetRing.Enabled)
            {
                DragFloat("Target Yalms", ref Profile.TargetRing.Radius, .25f, 1, 50);
                DragFloat("Target Thickness", ref Profile.TargetRing.Brush.Thickness, 1, 1, 50);
                ImGui.ColorEdit4("Target color", ref Profile.TargetRing.Brush.Color, ImGuiColorEditFlags.NoInputs);
            }

        }

        void TabProfile()
        {
            if (ImGui.Button("New Profile"))
            {
                var profile = new ConfigurationProfile("New Profile");
                Profiles.Add(profile);
                ConfigManager.Config.Active = profile;
            }

            if (Profiles.Count > 1)
            {
                ImGui.SameLine();
                if (ImGui.Button("Remove Profile"))
                {
                    Profiles.Remove(ConfigManager.ActiveProfile);
                    ConfigManager.Config.Active = Profiles.First();
                }
            }

            if (ImGui.BeginCombo("Current Profile", ConfigManager.Config.Active.Name))
            {
                foreach (var profile in ConfigManager.Config.Profiles)
                {
                    if (ImGui.Selectable($"{profile.Name}##{profile.ID}"))
                    {
                        ConfigManager.Config.Active = profile;
                        SetProfileNamePrompt(profile.Name);
                    }
                }
                ImGui.EndCombo();
            }

            if (ImGui.InputText("Profile Name", PromptProfileName, (uint)PromptProfileName.Length))
            {
                // update current profile name
                Profile.Name = Encoding.Default.GetString(PromptProfileName);
            }
            else
            {
                SetProfileNamePrompt(ConfigManager.Config.Active.Name);
            }

            ImGui.Text("Use profile for jobs:");
            foreach (var (classJob, index) in GetCombatClassJobs().Select((job, ndx) => (job, ndx)))
            {
                // todo: combine pre-job classes with the job like GLA/PLD and MRD/WAR
                // special case: ACN->SMN (not SCH)
                if ((index) % 3 != 0)
                {
                    ImGui.SameLine();
                }

                var isChecked = ConfigManager.ActiveProfile.Jobs.Contains(classJob.Abbreviation);
                var assignedProfile = ConfigManager.Config.ProfileForClassJob(classJob.Abbreviation);

                if (assignedProfile != null && assignedProfile != Profile)
                {
                    var unchanging = false;
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                    ImGui.Checkbox(classJob.Abbreviation, ref unchanging);
                    ImGui.PopStyleVar();
                    if (ImGui.IsItemHovered())
                    {
                        ImGui.SetTooltip($"Assigned to profile: {assignedProfile.Name}");
                    }
                }
                else
                {
                    if (ImGui.Checkbox(classJob.Abbreviation, ref isChecked))
                    {
                        if (isChecked)
                        {
                            ConfigManager.ActiveProfile.Jobs.Add(classJob.Abbreviation);
                        }
                        else
                        {
                            ConfigManager.ActiveProfile.Jobs.Remove(classJob.Abbreviation);
                        }
                    }
                }
            }
        }

        void SetProfileNamePrompt(string name)
        {
            Array.Clear(PromptProfileName, 0, PromptProfileName.Length);
            Encoding.UTF8.GetBytes(name, 0, name.Length, PromptProfileName, 0);
        }

        void TabDebug()
        {
            ImGui.Checkbox("Debug", ref ConfigManager.Config.Debug);
        }

        void DrawViewportConfigWindow()
        {
            if (!ConfigManager.ViewportUIVisible)
            {
                return;
            }

            var displaySize = ImGui.GetIO().DisplaySize;
            var windowSize = Config.ViewportWindowBox.SizeWith(displaySize);

            ImGui.SetNextWindowPos(Config.ViewportWindowBox.TopLeft, ImGuiCond.Appearing);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Always);

            if (ImGui.Begin("Viewport Window", ref ConfigManager.ViewportUIVisible))
            {
                ImGui.Text($"Drag and resize this window. Resonant will not draw outside of these bounds.");
                if (ImGui.Button($"Save and close"))
                {
                    ConfigManager.Save();
                    ConfigManager.ViewportUIVisible = false;
                }

                ImGui.Separator();

                if (ImGui.Button("Maximize"))
                {
                    Config.ViewportWindowBox = new();
                    ImGui.SetWindowPos(Config.ViewportWindowBox.TopLeft);
                }
                else if (ImGui.Button("Max Horizontal"))
                {
                    Config.ViewportWindowBox.TopLeft.X = 0;
                    Config.ViewportWindowBox.BottomRight.X = 0;
                    ImGui.SetWindowPos(Config.ViewportWindowBox.TopLeft);
                }
                else
                {
                    Config.ViewportWindowBox.TopLeft = ImGui.GetWindowPos();
                    Config.ViewportWindowBox.BottomRight = displaySize - ImGui.GetWindowPos() - ImGui.GetWindowSize();
                }

                ImGui.Separator();

                ImGui.Text($"Settings");
                ImGui.Text($"Top left: {Config.ViewportWindowBox.TopLeft}");
                ImGui.Text($"Bottom right: {Config.ViewportWindowBox.BottomRight}");
                ImGui.Text($"Size: {Config.ViewportWindowBox.SizeWith(displaySize)}");

                ImGui.Separator();

                ImGui.Text($"Display Size: {displaySize}");
                ImGui.Text($"Current window size: {ImGui.GetWindowSize()}");

            }
            ImGui.End();
        }

        // decorators to put label on preceding line
        void DragFloat(string label, ref float v, float v_speed, float v_min, float v_max)
        {
            ImGui.Text(Regex.Replace(label, "##\\w+", ""));
            ImGui.DragFloat($"##{label}", ref v, v_speed, v_min, v_max);
        }

        void DragInt(string label, ref int v, int v_speed, int v_min, int v_max)
        {
            ImGui.Text(Regex.Replace(label, "##\\w+", ""));
            ImGui.DragInt($"##{label}", ref v, v_speed, v_min, v_max);
        }

        List<ClassJob> GetCombatClassJobs()
        {
            return DataManager
                .GetExcelSheet<ClassJob>()!
                .Where(
                    (j) => j.Role != 0
                )
                .ToList();
        }
    }
}
