using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiNET;
using System;
using System.Numerics;

namespace Resonant
{
    internal class DebugUI : IDisposable, IDrawable
    {
        private Configuration config;
        private ClientState clientState;

        public DebugUI(Configuration config, ClientState clientState)
        {
            this.config = config;
            this.clientState = clientState;
        }

        public void Dispose()
        {
            // nothing to dispose
        }

        public void Draw()
        {
            var player = clientState.LocalPlayer;
            var target = clientState.LocalPlayer?.TargetObject;

            if (!player || !config.DebugUIVisible) { return; }

            ImGui.SetNextWindowSize(new Vector2(300, 300), ImGuiCond.Always);
            if (ImGui.Begin("Resonant Debug", ref config.DebugUIVisible))
            {
                ImGui.Text($"Player hitbox: {player!.HitboxRadius}");

                if (target != null)
                {
                    ImGui.Text($"== Target ==");
                    var distance = Distance(player, target);
                    ImGui.Text($"XZ Distance: {distance}");
                    ImGui.Text($"Hitbox: {target.HitboxRadius}");
                    ImGui.Text($"YalmDistance: X: {target.YalmDistanceX} Z: {target.YalmDistanceZ}");
                    ImGui.Text($"Objectkind: {target.ObjectKind}");
                    ImGui.Text($"Subkind: {target.SubKind}");
                    ImGui.Text($"Type: {target.GetType()}");

                    var battle = target as BattleNpc;
                    if (battle != null)
                    {
                        ImGui.Text($"Kind: {battle.BattleNpcKind}");
                        ImGui.Text($"Custom: {battle.Customize}");
                        ImGui.Text($"StatusFlags: {battle.StatusFlags}");
                        ImGui.Text($"WTB: the flag that says positionals aren't required");
                    }
                    else
                    {
                        ImGui.Text($"Not battle NPC");
                    }
                }
            }
            ImGui.End();
        }

        private float Distance(GameObject? a, GameObject? b)
        {
            if (a == null || b == null) { return 0f; }

            var dx = b.Position.X - a.Position.X;
            var dz = b.Position.Z - a.Position.Z;
            return (float)Math.Sqrt(dx * dx + dz * dz);
        }
    }
}
