using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Gui;
using Dalamud.Logging;
using ImGuiNET;
using System;

namespace Resonant
{
    internal class ResonantCore : IDisposable, IDrawable
    {
        private const float RangeAutoAttack = 2.1f;
        private const float RangeAbilityMelee = 3f;

        private Configuration config;
        private ClientState clientState;
        private GameGui gui;
        private Canvas canvas;

        public ResonantCore(Configuration config, ClientState clientState, GameGui gui)
        {
            this.config = config;
            this.clientState = clientState;
            this.gui = gui;

            canvas = new Canvas(this.config, this.gui);

            Initialize();
        }

        internal void Initialize()
        {
            // TODO
        }

        public void Draw()
        {
            var player = clientState.LocalPlayer;
            if (player == null)
            {
                return;
            }

            canvas.Begin();

            if (config.PlayerRing.Enabled)
            {
                DrawPlayerRing(player);
            }

            if (config.Cone.Enabled)
            {
                DrawPlayerCone(player);
            }

            if (config.TargetRing.Enabled)
            {
                DrawTargetRing(player);
            }

            if (config.Positionals.Enabled)
            {
                DrawPositionals(player);
            }

            if (config.Hitbox.Enabled)
            {
                DrawHitbox(player);
            }

            ImGui.End();
            ImGui.PopStyleVar();
        }

        private void DrawHitbox(Character player)
        {
            var pos = player.Position;
            var c = config.Hitbox;
            if (player.TargetObject != null)
            {
                // Make the hitbox dot on the same Y-plane as the other effects we may draw;
                // otherwise it get rather confusing to look at
                pos.Y = player.TargetObject.Position.Y;

                if (c.ShowDeltaY)
                {
                    canvas.Segment(pos, player.Position, new(c.Color, 2));
                }
            }

            canvas.CircleXZ(pos, .02f, new(c.OutlineColor, 5));
            canvas.CircleXZ(pos, .01f, new(c.Color, 4));
        }

        private void DrawPlayerRing(Character player)
        {
            var c = config.PlayerRing;
            canvas.CircleXZ(player.Position, c.Radius, c.Brush);
        }

        private void DrawPlayerCone(Character player)
        {
            // rotate arc towards target (if exists)
            var c = config.Cone;
            var target = player.TargetObject;
            var rotation = target != null
                ? Math.Atan2(target.Position.X - player.Position.X, target.Position.Z - player.Position.Z)
                : player.Rotation;

            canvas.ConeCenteredXZ(player.Position, c.Radius, (float)rotation, Maths.Radians(c.Angle), c.Brush);
        }

        private void DrawTargetRing(Character player)
        {
            if (player.TargetObject != null)
            {
                canvas.CircleXZ(player.TargetObject.Position, config.TargetRing.Radius, config.TargetRing.Brush);
            }
        }

        private void DrawPositionals(Character player)
        {
            var c = config.Positionals;
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

            var regionBrushes = Regions.FromConfig(c, melee, ability);

            if (c.ArrowEnabled)
            {
                DrawEnemyArrow(target, 0, melee);
            }

            // TODO: If the target doesn't need positionals then don't draw sectors
            foreach (var (region, brush) in regionBrushes)
            {
                canvas.ActorConeXZ(
                    target,
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

                        canvas.ActorDonutSliceXZ(
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
            var c = config.Positionals;
            canvas.ActorArrowXZ(target, pointRadius, angle, c.ArrowScale, c.BrushFront);
        }

        public void Dispose()
        {
            canvas.Dispose();
        }

        internal void Debug(String message, params object[] values)
        {
            if (config.DebugUIVisible)
            {
                PluginLog.Log(message, values);
            }
        }
    }
}
