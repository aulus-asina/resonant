using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Logging;
using ImGuiNET;
using System;

namespace Resonant
{
    internal class ResonantCore : IDrawable
    {
        private const float RangeAutoAttack = 2.1f;
        private const float RangeAbilityMelee = 3f;

        private ConfigurationManager ConfigManager;
        private ClientState ClientState;
        private GameGui Gui;
        private Canvas Canvas;
        private GameStateObserver GameStateObserver;

        private ConfigurationProfile Profile
        {
            get { return ConfigManager.Config.Active; }
        }

        public ResonantCore(ConfigurationManager configManager, ClientState clientState, GameGui gui, DataManager dataManager)
        {
            ConfigManager = configManager;
            ClientState = clientState;
            Gui = gui;
            Canvas = new Canvas(ConfigManager.Config, Gui);
            GameStateObserver = new(clientState, dataManager);

            Initialize();
        }

        internal void Initialize()
        {
            GameStateObserver.JobChangedEvent += OnJobChange;
        }

        public void Draw()
        {
            GameStateObserver.Observe();

            var player = ClientState.LocalPlayer;
            if (player == null)
            {
                return;
            }

            Canvas.Begin();

            if (Profile.PlayerRing.Enabled)
            {
                DrawPlayerRing(player);
            }

            if (Profile.Cone.Enabled)
            {
                DrawPlayerCone(player);
            }

            if (Profile.TargetRing.Enabled)
            {
                DrawTargetRing(player);
            }

            if (Profile.Positionals.Enabled)
            {
                DrawPositionals(player);
            }

            if (Profile.Hitbox.Enabled)
            {
                DrawHitbox(player);
            }

            ImGui.End();
            ImGui.PopStyleVar();
        }

        private void DrawHitbox(Character player)
        {
            var pos = player.Position;
            var c = Profile.Hitbox;

            if (c.UseTargetY && player.TargetObject != null)
            {
                pos.Y = player.TargetObject.Position.Y;

                if (c.ShowTargetDeltaY)
                {
                    Canvas.Segment(pos, player.Position, new(c.Color, 2));
                }
            }

            Canvas.CircleXZ(pos, .02f, new(c.OutlineColor, 5));
            Canvas.CircleXZ(pos, .01f, new(c.Color, 4));
        }

        private void DrawPlayerRing(Character player)
        {
            var c = Profile.PlayerRing;
            Canvas.CircleXZ(player.Position, c.Radius, c.Brush);
        }

        private void DrawPlayerCone(Character player)
        {
            // rotate arc towards target (if exists)
            var c = Profile.Cone;
            var target = player.TargetObject;
            var rotation = target != null
                ? Math.Atan2(target.Position.X - player.Position.X, target.Position.Z - player.Position.Z)
                : player.Rotation;

            Canvas.ConeCenteredXZ(player.Position, c.Radius, (float)rotation, Maths.Radians(c.Angle), c.Brush);
        }

        private void DrawTargetRing(Character player)
        {
            if (player.TargetObject != null)
            {
                Canvas.CircleXZ(player.TargetObject.Position, Profile.TargetRing.Radius, Profile.TargetRing.Brush);
            }
        }

        private void DrawPositionals(Character player)
        {
            var c = Profile.Positionals;
            var target = player.TargetObject;

            // don't draw positionals if not targeting a battle mob
            if (target == null || target.ObjectKind != ObjectKind.BattleNpc)
            {
                return;
            }

            // annoyingly, the hitbox size changes on mounts. maybe detect and hardcode, its a slight annoyance in the world
            var playerHitbox = player.HitboxRadius;

            // for an ability to be in range, the character's hitbox (plus the
            // range of the attack) has to overlap with the target's hitbox
            var hitboxes = playerHitbox + target.HitboxRadius;
            var melee = hitboxes + RangeAutoAttack; // XXX: is this fully accurate? is there a real analysis around this value?
            var ability = hitboxes + RangeAbilityMelee;
            var abilityfar = hitboxes + c.FarAbilityRange;

            var regionBrushes = Regions.FromConfig(c, melee, ability, abilityfar);

            if (c.ArrowEnabled)
            {
                DrawEnemyArrow(target, 0, melee);
            }

            // TODO: If the target doesn't need positionals then don't draw sectors
            foreach (var (region, brush) in regionBrushes)
            {
                Canvas.ActorDonutSliceXZ(
                    target,
                    region.Radius.Inner,
                    region.Radius.Outer,
                    region.Positional.StartRads,
                    region.Positional.EndRads,
                    brush
                );
            }

            if (c.HighlightCurrentRegion)
            {
                var targetActor = new Actor(target);
                foreach (var (region, brush) in regionBrushes)
                {
                    if (targetActor.regionContains(region, player.Position))
                    {
                        var fillBrush = brush with
                        {
                            Fill = brush.Color with
                            {
                                W = brush.Color.W * c.HighlightTransparencyMultiplier
                            }
                        };

                        Canvas.ActorDonutSliceXZ(
                            target,
                            region.Radius.Inner, region.Radius.Outer,
                            region.Positional.StartRads, region.Positional.EndRads,
                            fillBrush
                        );
                        break;
                    }
                }
            }
        }

        private void DrawEnemyArrow(GameObject target, float angle, float pointRadius)
        {
            var c = Profile.Positionals;
            Canvas.ActorArrowXZ(target, pointRadius, angle, c.ArrowScale, c.BrushFront);
        }

        internal void Debug(String message, params object[] values)
        {
            if (ConfigManager.Config.Debug)
            {
                PluginLog.Log(message, values);
            }
        }

        private void OnJobChange(object sender, string classJobAbbrev)
        {
            Dalamud.Logging.PluginLog.Log($"Detected class change: {classJobAbbrev}");

            var profile = ConfigManager.Config.ProfileForClassJob(classJobAbbrev);
            if (profile != null) {
                ConfigManager.Config.Active = profile;
            }
        }
    }
}
